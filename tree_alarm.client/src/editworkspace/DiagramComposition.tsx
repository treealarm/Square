import { useEffect, useState } from 'react';
import { Box, List, ListItem, ListItemButton, ListItemText, Typography } from '@mui/material';
import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import { useAppDispatch } from '../store/configureStore';
import { TreeMarker } from '../store/Marker';
import { TREE_MARKER_DRAG_TYPE, getDragGhostImage } from '../tree/dragTypes';

export interface IDiagramCompositionProps {
  diagramId: string;
  // Bump this from the parent after attaching a new object, to force a refetch.
  refreshKey: number;
}

// "Diagram composition" — the diagram's own children in the object tree (the same objects
// DiagramElement.tsx already renders as nested diagram entries). Lets you grab one and drag it
// straight onto the canvas, without hunting for it again in the full tree.
export function DiagramComposition({ diagramId, refreshKey }: IDiagramCompositionProps) {
  const appDispatch = useAppDispatch();
  const [children, setChildren] = useState<TreeMarker[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    TreeStore.getByParent(diagramId, null, null)
      .then((res) => {
        if (!cancelled) setChildren(res.children ?? []);
      })
      .catch(() => {
        if (!cancelled) setChildren([]);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [diagramId, refreshKey]);

  return (
    <Box>
      <Typography variant="subtitle2" sx={{ px: 1, pt: 1 }}>Diagram composition</Typography>
      {!loading && children.length === 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ px: 1, pb: 1 }}>
          Empty — attach objects using the search on the left
        </Typography>
      )}
      <List dense>
        {children.map((child) => (
          <ListItem key={child.id} disablePadding>
            <ListItemButton
              draggable
              onClick={() => appDispatch(GuiStore.selectTreeItem(child.id))}
              onDragStart={(e) => {
                e.dataTransfer.setData(TREE_MARKER_DRAG_TYPE, child.id ?? '');
                e.dataTransfer.setDragImage(getDragGhostImage(), 12, 12);
              }}
            >
              <ListItemText primary={child.name} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
    </Box>
  );
}
