import { useCallback, useEffect, useRef, useState } from 'react';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../../store';
import { useAppDispatch } from '../../store/configureStore';
import * as TreeStore from '../../store/TreeStates';
import * as GuiStore from '../../store/GUIStates';
import * as IntegroStore from '../../store/IntegroStates';
import * as ObjPropsStore from '../../store/ObjPropsStates';
import { IObjProps, IUpdateIntegroObjectDTO, TreeMarker } from '../../store/Marker';
import { createObjectsFromBrowserFile } from '../../tree/createObjectsFromBrowserFile';
import {
  TreeCache,
  emptyCache,
  mergeChildPage,
  nodeChildIds,
  nodeContainsId,
  nodeHasMore,
  nodeNextStartId,
  resetNodeChildren,
  resetRoot,
  setExpanded,
  setNodeLoading,
  setRootLoading,
} from './treeNodeCache';

// Safety cap on how many sibling pages we'll scan looking for one specific id while revealing a
// search result — guards against a runaway loop rather than assuming it can't happen.
const REVEAL_PAGE_SAFETY_LIMIT = 20;

export function useEditTree() {
  const appDispatch = useAppDispatch();
  const [cache, setCache] = useState<TreeCache>(emptyCache);
  const reduxSelectedId = useSelector((state: ApplicationState) => state.guiStates?.selected_id) ?? null;
  const objectIntegroType = useSelector(
    (state: ApplicationState) => state?.integroStates?.integroType ?? null
  );

  const rowRefs = useRef<Map<string, HTMLElement>>(new Map());
  const pendingScrollId = useRef<string | null>(null);

  const loadChildren = useCallback(async (parentId: string | null, startId: string | null) => {
    setCache((c) => (parentId == null ? setRootLoading(c, true) : setNodeLoading(c, parentId, true)));
    const page = await TreeStore.getByParent(parentId, startId, null);
    setCache((c) => mergeChildPage(c, parentId, page));
  }, []);

  useEffect(() => {
    loadChildren(null, null);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const toggleExpand = useCallback(
    (node: TreeMarker) => {
      if (!node.id || !node.has_children) return;
      const id = node.id;
      setCache((c) => setExpanded(c, id, !c.nodes[id]?.expanded));
      if (nodeChildIds(cache, id) == null) {
        loadChildren(id, null);
      }
    },
    [cache, loadChildren]
  );

  const loadMore = useCallback(
    (parentId: string | null) => {
      loadChildren(parentId, nodeNextStartId(cache, parentId));
    },
    [cache, loadChildren]
  );

  const selectItem = useCallback(
    (node: TreeMarker | null) => {
      const id = node?.id === reduxSelectedId ? null : node?.id ?? null;
      appDispatch(GuiStore.selectTreeItem(id));
    },
    [appDispatch, reduxSelectedId]
  );

  const refreshNode = useCallback(
    (id: string | null) => {
      setCache((c) => (id == null ? resetRoot(c) : resetNodeChildren(c, id)));
      loadChildren(id, null);
    },
    [loadChildren]
  );

  const addChildItem = useCallback(
    async (type?: string | null) => {
      const parentId = reduxSelectedId;
      const copy: IObjProps = {
        id: null,
        name: type ? `new ${type}` : 'new object',
        parent_id: parentId,
      };

      try {
        if (type && objectIntegroType) {
          const newObj: IUpdateIntegroObjectDTO = {
            obj: copy,
            integro: { i_name: objectIntegroType.i_name, i_type: type },
          };
          await appDispatch(IntegroStore.updateIntegroObject(newObj)).unwrap();
        } else {
          await appDispatch(ObjPropsStore.updateObjProps(copy)).unwrap();
        }
      } catch (err) {
        // eslint-disable-next-line no-console
        console.error('useEditTree: failed to add child', err);
        return;
      }
      refreshNode(parentId);
    },
    [appDispatch, objectIntegroType, reduxSelectedId, refreshNode]
  );

  const importFromFile = useCallback(
    (file: File) => {
      createObjectsFromBrowserFile(file, reduxSelectedId, appDispatch, objectIntegroType?.i_name ?? null);
    },
    [appDispatch, reduxSelectedId, objectIntegroType]
  );

  const registerRowRef = useCallback((id: string, el: HTMLElement | null) => {
    if (el) rowRefs.current.set(id, el);
    else rowRefs.current.delete(id);
  }, []);

  useEffect(() => {
    if (pendingScrollId.current && pendingScrollId.current === reduxSelectedId) {
      const el = rowRefs.current.get(pendingScrollId.current);
      if (el) {
        el.scrollIntoView({ block: 'center' });
        pendingScrollId.current = null;
      }
    }
  }, [reduxSelectedId, cache]);

  // Reveals a flat search result inside the nested tree: expands every ancestor from root down
  // to the item's parent, paginating each level (capped) until the next id in the chain is
  // actually present, then selects the item and scrolls its row into view.
  const revealAndSelect = useCallback(
    async (item: TreeMarker) => {
      if (!item.id) return;
      let local = cache;

      const ensureContains = async (parentId: string | null, targetId: string) => {
        if (nodeChildIds(local, parentId) == null) {
          const page = await TreeStore.getByParent(parentId, null, null);
          local = mergeChildPage(local, parentId, page);
          setCache(local);
        }
        let pageCount = 1;
        while (
          !nodeContainsId(local, parentId, targetId) &&
          nodeHasMore(local, parentId) &&
          pageCount < REVEAL_PAGE_SAFETY_LIMIT
        ) {
          const page = await TreeStore.getByParent(parentId, nodeNextStartId(local, parentId), null);
          local = mergeChildPage(local, parentId, page);
          setCache(local);
          pageCount += 1;
        }
        if (!nodeContainsId(local, parentId, targetId)) {
          // eslint-disable-next-line no-console
          console.warn(
            `useEditTree: could not locate ${targetId} under ${parentId ?? 'root'} after ${pageCount} page(s)`
          );
        }
      };

      if (item.parent_id == null) {
        await ensureContains(null, item.id);
        pendingScrollId.current = item.id;
        selectItem(item);
        return;
      }

      const page = await TreeStore.getByParent(item.parent_id, null, null);
      local = mergeChildPage(local, item.parent_id, page);
      setCache(local);

      // GetByChildIdAsync always includes the queried node itself as the chain's last element,
      // so `page.parents` is already the full root → item.parent_id chain — no extra fetch needed.
      const chain: TreeMarker[] = page.parents ?? [];

      if (chain.length > 0 && chain[0].id) {
        await ensureContains(null, chain[0].id);
      }

      for (let i = 0; i < chain.length; i++) {
        const node = chain[i];
        if (!node.id) continue;
        local = setExpanded(local, node.id, true);
        setCache(local);
        const nextId = i + 1 < chain.length ? chain[i + 1].id : item.id;
        if (nextId) {
          await ensureContains(node.id, nextId);
        }
      }

      pendingScrollId.current = item.id;
      selectItem(item);
    },
    [cache, selectItem]
  );

  return {
    cache,
    reduxSelectedId,
    objectIntegroType,
    toggleExpand,
    loadMore,
    selectItem,
    addChildItem,
    importFromFile,
    refreshNode,
    revealAndSelect,
    registerRowRef,
  };
}

export type UseEditTreeApi = ReturnType<typeof useEditTree>;
