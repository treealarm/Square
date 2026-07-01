import { useState } from 'react';
import {
  Box, CircularProgress, IconButton, List, ListItem, ListItemButton, ListItemText,
  Menu, MenuItem, Tooltip,
} from '@mui/material';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import AddIcon from '@mui/icons-material/Add';
import MenuOpenIcon from '@mui/icons-material/MenuOpen';
import UploadFileIcon from '@mui/icons-material/UploadFile';

import { TreeMarker } from '../../store/Marker';
import { TREE_MARKER_DRAG_TYPE, getDragGhostImage } from '../../tree/dragTypes';
import { UseEditTreeApi } from './useEditTree';

export interface IEditTreeRowProps {
  node: TreeMarker;
  depth: number;
  // Whether this row is the last among its siblings — its own vertical guide stops at its
  // center (the classic "L" corner) instead of continuing down to a next sibling.
  isLast: boolean;
  // For each ancestor level above this row's own (index 0 = depth 1, ...), whether that
  // ancestor still has more siblings below it and so needs its vertical guide line to pass
  // straight through this row instead of stopping.
  ancestorContinues: boolean[];
  treeApi: UseEditTreeApi;
}

const BASE_PADDING_PX = 16;
const INDENT_PX = 24;
const LINE_COLOR = 'divider';

// X position (px) of the vertical guide line that belongs to a row at this depth.
function lineX(depth: number) {
  return BASE_PADDING_PX + (depth - 0.5) * INDENT_PX;
}

export function EditTreeRow({ node, depth, isLast, ancestorContinues, treeApi }: IEditTreeRowProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  if (!node.id) return null;

  const nodeId = node.id;
  const nodeState = treeApi.cache.nodes[nodeId];
  const isSelected = treeApi.reduxSelectedId === nodeId;
  const isExpanded = nodeState?.expanded ?? false;
  const childTypes = treeApi.objectIntegroType?.children ?? [];
  const childIds = nodeState?.childIds ?? [];

  return (
    <>
      <ListItem
        disablePadding
        sx={{ position: 'relative' }}
        secondaryAction={
          <>
            {node.has_children && (
              <IconButton size="small" edge="end" onClick={() => treeApi.toggleExpand(node)}>
                {isExpanded ? <ExpandMoreIcon /> : <ChevronRightIcon />}
              </IconButton>
            )}

            {isSelected && (
              <>
                <Tooltip title="Add new child">
                  <IconButton size="small" edge="end" onClick={() => treeApi.addChildItem(null)}>
                    <AddIcon />
                  </IconButton>
                </Tooltip>

                {childTypes.length > 0 && (
                  <>
                    <Tooltip title="Add new typed child">
                      <IconButton size="small" edge="end" onClick={(e) => setAnchorEl(e.currentTarget)}>
                        <MenuOpenIcon />
                      </IconButton>
                    </Tooltip>
                    <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={() => setAnchorEl(null)}>
                      {childTypes.map((type) => (
                        <MenuItem
                          key={type.child_i_type}
                          onClick={() => {
                            treeApi.addChildItem(type.child_i_type);
                            setAnchorEl(null);
                          }}
                        >
                          Add integration child [{type.child_i_type}]
                        </MenuItem>
                      ))}
                      <MenuItem>
                        <label
                          style={{
                            display: 'flex', alignItems: 'center', cursor: 'pointer',
                            width: '100%', height: '100%',
                          }}
                        >
                          <UploadFileIcon fontSize="small" style={{ marginRight: 8 }} />
                          Import from file
                          <input
                            type="file"
                            accept=".json"
                            onChange={(e) => {
                              if (e.target.files?.[0]) {
                                treeApi.importFromFile(e.target.files[0]);
                                e.target.value = '';
                              }
                            }}
                            style={{ display: 'none' }}
                          />
                        </label>
                      </MenuItem>
                    </Menu>
                  </>
                )}
              </>
            )}
          </>
        }
      >
        {ancestorContinues.map((continues, i) => continues && (
          <Box
            key={i}
            aria-hidden
            sx={{
              position: 'absolute', left: lineX(i + 1), top: 0, bottom: 0,
              width: '1px', bgcolor: LINE_COLOR, pointerEvents: 'none',
            }}
          />
        ))}
        {depth > 0 && (
          <>
            <Box
              aria-hidden
              sx={{
                position: 'absolute', left: lineX(depth), top: 0,
                height: isLast ? '50%' : '100%',
                width: '1px', bgcolor: LINE_COLOR, pointerEvents: 'none',
              }}
            />
            <Box
              aria-hidden
              sx={{
                position: 'absolute', left: lineX(depth), top: '50%',
                width: INDENT_PX / 2, height: '1px', bgcolor: LINE_COLOR, pointerEvents: 'none',
              }}
            />
          </>
        )}

        <ListItemButton
          sx={{ pl: `${BASE_PADDING_PX + depth * INDENT_PX}px` }}
          selected={isSelected}
          onClick={() => treeApi.selectItem(node)}
          draggable
          onDragStart={(e) => {
            e.dataTransfer.setData(TREE_MARKER_DRAG_TYPE, nodeId);
            e.dataTransfer.setDragImage(getDragGhostImage(), 12, 12);
          }}
          ref={(el: HTMLDivElement | null) => treeApi.registerRowRef(nodeId, el)}
        >
          <ListItemText primary={node.name} />
        </ListItemButton>
      </ListItem>

      {isExpanded && (
        <List dense disablePadding>
          {childIds.map((childId, index) => {
            const childMarker = treeApi.cache.nodes[childId]?.marker;
            return childMarker ? (
              <EditTreeRow
                key={childId}
                node={childMarker}
                depth={depth + 1}
                isLast={index === childIds.length - 1}
                ancestorContinues={[...ancestorContinues, !isLast]}
                treeApi={treeApi}
              />
            ) : null;
          })}
          {nodeState?.loading && (
            <Box sx={{ pl: `${BASE_PADDING_PX + (depth + 1) * INDENT_PX}px`, py: 0.5, display: 'flex', alignItems: 'center', gap: 1 }}>
              <CircularProgress size={14} />
            </Box>
          )}
          {!nodeState?.loading && nodeState?.hasMore && (
            <ListItemButton
              sx={{ pl: `${BASE_PADDING_PX + (depth + 1) * INDENT_PX}px` }}
              onClick={() => treeApi.loadMore(nodeId)}
            >
              <ListItemText primary="Load more..." />
            </ListItemButton>
          )}
        </List>
      )}
    </>
  );
}
