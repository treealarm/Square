/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useCallback } from 'react';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import { IObjProps, TreeMarker } from '../store/Marker';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { Box, Toolbar, Tooltip } from '@mui/material';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AddIcon from '@mui/icons-material/Add';
import TabControl from './TabControl';
import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import * as ObjPropsStore from '../store/ObjPropsStates';
import { useAppDispatch } from "../store/configureStore";

export function TreeControl() {
  const appDispatch = useAppDispatch();

  const treeStates = useSelector((state: ApplicationState) => state.treeStates);
  const parentBounds = useSelector((state: ApplicationState) => state.treeStates?.parentBounds || {});

  const markers = useSelector((state: ApplicationState) => state.treeStates?.children);
  const parentMarkerId = useSelector((state: ApplicationState) => state.treeStates?.parent_id);
  const user = useSelector((state: ApplicationState) => state.rightsStates?.user);
  const reduxSelectedId = useSelector((state: ApplicationState) => state.guiStates?.selected_id);
  const requestTreeUpdate = useSelector((state: ApplicationState) => state.guiStates?.requestedTreeUpdate);

  const getTreeItemsByParent = useCallback((parentId: string | null) => {
    appDispatch(TreeStore.fetchByParent({ parent_id: parentId, start_id: null, end_id: null }));
  }, [appDispatch]);

  const startEndBounds = parentMarkerId ? parentBounds[parentMarkerId] : parentBounds[''];

  useEffect(() => {
    getTreeItemsByParent(null);
  }, [user]);

  useEffect(() => {
    appDispatch(TreeStore.fetchByParent({
      parent_id: parentMarkerId ?? null,
      start_id: startEndBounds?.start_id ?? null,
      end_id: startEndBounds?.end_id ?? null
    }));
  }, [requestTreeUpdate, parentMarkerId]);

  const [checked, setChecked] = React.useState<Set<string>>(new Set());

  const handleChecked = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedId = event.target.id;
    const newChecked = new Set(checked);
    if (event.target.checked) {
      newChecked.add(selectedId);
    } else {
      newChecked.delete(selectedId);
    }
    setChecked(newChecked);
    appDispatch(GuiStore.checkTreeItem(Array.from(newChecked)));
  };

  const selectItem = (selectedMarker: TreeMarker | null) => {
    const selectedId = selectedMarker?.id === reduxSelectedId ? null : selectedMarker?.id ?? null;
    appDispatch(GuiStore.selectTreeItem(selectedId));
  };

  const handleSelect = (selected: TreeMarker) => () => selectItem(selected);

  const drillDown = (selectedMarker: TreeMarker | null) => () => {
    selectItem(null);
    getTreeItemsByParent(selectedMarker?.id ?? null);
  };

  const onNavigate = (next: boolean) => {
    appDispatch(TreeStore.fetchByParent({
      parent_id: parentMarkerId ??  null,
      start_id: next ? startEndBounds?.end_id : null,
      end_id: next ? null : startEndBounds?.start_id
    }));
  };

  const addChildItem = () => {

    let copy: IObjProps = {
      id: null,
      name: 'new object',
      parent_id: reduxSelectedId
    }
    appDispatch(ObjPropsStore.updateObjProps(copy));

    appDispatch(TreeStore.fetchByParent({
      parent_id: parentMarkerId ?? null,
      start_id: treeStates?.start_id ?? null,
      end_id: null
    }));
  };

  return (
    <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
      <TabControl />
      <Box sx={{ flexGrow: 1, backgroundColor: 'lightgray' }}>
        <Toolbar variant="dense">
          <Tooltip title="Go to previous page">
            <IconButton onClick={() => onNavigate(false)}>
              <ArrowBackIcon />
            </IconButton>
          </Tooltip>
          <Box sx={{ flexGrow: 1 }} />
          {reduxSelectedId == null ? (
            <Tooltip title="Add new object">
              <IconButton onClick={addChildItem}>
                <AddIcon />
              </IconButton>
            </Tooltip>
          ) : null}
          <Tooltip title="Go to next page">
            <IconButton onClick={() => onNavigate(true)}>
              <ArrowForwardIcon />
            </IconButton>
          </Tooltip>
        </Toolbar>
      </Box>
      <Box sx={{ width: '100%', height: '100%', overflow: 'auto' }}>
        <List dense sx={{ minHeight: '100%', width: '100%' }}>
          {markers?.map((marker) => (
            <ListItem key={marker.id} disablePadding secondaryAction={
              <>
                {marker.has_children && (
                  <IconButton size="small" edge="end" aria-label="drill_down" onClick={drillDown(marker)}>
                    <ChevronRightIcon />
                  </IconButton>
                )}
                {reduxSelectedId === marker.id && (
                  <Tooltip title="Add new child">
                    <IconButton size="small" edge="end" aria-label="add_child" onClick={addChildItem}>
                      <AddIcon />
                    </IconButton>
                  </Tooltip>
                )}
              </>
            }>
              <ListItemButton selected={reduxSelectedId === marker.id} onClick={handleSelect(marker)}>
                <ListItemIcon>
                  <Checkbox
                    size="small"
                    edge="start"
                    checked={checked.has(marker.id)}
                    tabIndex={-1}
                    disableRipple
                    id={marker.id}
                    onChange={handleChecked}
                  />
                </ListItemIcon>
                <ListItemText primary={marker.name} />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Box>
    </Box>
  );
}
