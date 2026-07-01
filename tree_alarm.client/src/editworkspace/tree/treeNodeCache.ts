import { GetByParentDTO, TreeMarker } from '../../store/Marker';

export const PAGE_SIZE = 100;

export interface TreeNodeState {
  marker: TreeMarker;
  childIds: string[] | null; // null = never fetched yet
  expanded: boolean;
  loading: boolean;
  hasMore: boolean;
  nextStartId: string | null;
}

export interface TreeCache {
  nodes: Record<string, TreeNodeState>;
  rootIds: string[];
  rootLoaded: boolean;
  rootLoading: boolean;
  rootHasMore: boolean;
  rootNextStartId: string | null;
}

export const emptyCache: TreeCache = {
  nodes: {},
  rootIds: [],
  rootLoaded: false,
  rootLoading: false,
  rootHasMore: false,
  rootNextStartId: null,
};

// Backend pagination is inclusive on start_id (MapService.GetByParentIdsAsync), so re-fetching
// with the last seen id as the next start_id returns that item again — drop it here instead of
// special-casing the cursor itself.
function dedupeAppend(existingIds: string[], newMarkers: TreeMarker[]): string[] {
  const seen = new Set(existingIds);
  const appended: string[] = [];
  for (const m of newMarkers) {
    if (m.id && !seen.has(m.id)) {
      seen.add(m.id);
      appended.push(m.id);
    }
  }
  return [...existingIds, ...appended];
}

export function upsertNode(cache: TreeCache, marker: TreeMarker): TreeCache {
  if (!marker.id) return cache;
  const existing = cache.nodes[marker.id];
  if (existing) {
    // `parents` (breadcrumb) entries come back as BaseMarkerDTO, which has no `has_children`
    // field at all — backend's GetByChildIdAsync also always includes the queried node itself
    // as the last breadcrumb element. Without this merge, expanding any node would silently
    // wipe its own `has_children` (already known from the richer MarkerDTO listing) and hide
    // its expand chevron.
    const merged: TreeMarker = {
      ...existing.marker,
      ...marker,
      has_children: marker.has_children ?? existing.marker.has_children,
    };
    return { ...cache, nodes: { ...cache.nodes, [marker.id]: { ...existing, marker: merged } } };
  }
  return {
    ...cache,
    nodes: {
      ...cache.nodes,
      [marker.id]: {
        marker,
        childIds: null,
        expanded: false,
        loading: false,
        hasMore: false,
        nextStartId: null,
      },
    },
  };
}

export function setExpanded(cache: TreeCache, id: string, expanded: boolean): TreeCache {
  const existing = cache.nodes[id];
  if (!existing) return cache;
  return { ...cache, nodes: { ...cache.nodes, [id]: { ...existing, expanded } } };
}

export function setNodeLoading(cache: TreeCache, id: string, loading: boolean): TreeCache {
  const existing = cache.nodes[id];
  if (!existing) return cache;
  return { ...cache, nodes: { ...cache.nodes, [id]: { ...existing, loading } } };
}

export function setRootLoading(cache: TreeCache, loading: boolean): TreeCache {
  return { ...cache, rootLoading: loading };
}

export function resetNodeChildren(cache: TreeCache, id: string): TreeCache {
  const existing = cache.nodes[id];
  if (!existing) return cache;
  return {
    ...cache,
    nodes: {
      ...cache.nodes,
      [id]: { ...existing, childIds: null, hasMore: false, nextStartId: null, expanded: true },
    },
  };
}

export function resetRoot(cache: TreeCache): TreeCache {
  return { ...cache, rootIds: [], rootLoaded: false, rootHasMore: false, rootNextStartId: null };
}

// Merges one fetched page of children into the cache, either at root (parentId == null) or
// under a specific node. Also caches any ancestor markers the backend handed back for free.
export function mergeChildPage(
  cache: TreeCache,
  parentId: string | null,
  page: GetByParentDTO
): TreeCache {
  const children = page.children ?? [];
  let next = cache;

  for (const ancestor of page.parents ?? []) {
    next = upsertNode(next, ancestor);
  }
  for (const child of children) {
    next = upsertNode(next, child);
  }

  const hasMore = children.length >= PAGE_SIZE;
  const lastId = children.length > 0 ? children[children.length - 1].id ?? null : null;

  if (parentId == null) {
    return {
      ...next,
      rootIds: dedupeAppend(next.rootIds, children),
      rootLoaded: true,
      rootLoading: false,
      rootHasMore: hasMore,
      rootNextStartId: lastId ?? next.rootNextStartId,
    };
  }

  const existing = next.nodes[parentId];
  if (!existing) return next;
  return {
    ...next,
    nodes: {
      ...next.nodes,
      [parentId]: {
        ...existing,
        childIds: dedupeAppend(existing.childIds ?? [], children),
        loading: false,
        hasMore,
        nextStartId: lastId ?? existing.nextStartId,
      },
    },
  };
}

export function nodeChildIds(cache: TreeCache, parentId: string | null): string[] | null {
  return parentId == null ? (cache.rootLoaded ? cache.rootIds : null) : (cache.nodes[parentId]?.childIds ?? null);
}

export function nodeContainsId(cache: TreeCache, parentId: string | null, id: string): boolean {
  return (nodeChildIds(cache, parentId) ?? []).includes(id);
}

export function nodeHasMore(cache: TreeCache, parentId: string | null): boolean {
  return parentId == null ? cache.rootHasMore : (cache.nodes[parentId]?.hasMore ?? false);
}

export function nodeNextStartId(cache: TreeCache, parentId: string | null): string | null {
  return parentId == null ? cache.rootNextStartId : (cache.nodes[parentId]?.nextStartId ?? null);
}

export function nodeIsLoading(cache: TreeCache, parentId: string | null): boolean {
  return parentId == null ? cache.rootLoading : (cache.nodes[parentId]?.loading ?? false);
}
