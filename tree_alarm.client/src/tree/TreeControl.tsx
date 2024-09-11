/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';
import { useEffect } from 'react';
import { useSelector } from "react-redux";
import { useAppDispatch } from "../store/configureStore";
import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { TreeMarker } from '../store/Marker';
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
import TabControl from './TabControl';

export function TreeControl() {
  const appDispatch = useAppDispatch();

  const getTreeItemsByParent = (parent_marker_id: string | null) => {
    appDispatch(TreeStore.fetchByParent({ parent_id: parent_marker_id, start_id: null, end_id: null }));
  };


  const treeStates = useSelector((state: ApplicationState) => state?.treeStates);
  const markers = useSelector((state: ApplicationState) => state?.treeStates?.children);
  const parent_marker_id = useSelector((state: ApplicationState) => state?.treeStates?.parent_id??null);
  const user = useSelector((state: ApplicationState) => state?.rightsStates?.user);
  const reduxSelectedId = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const requestTreeUpdate = useSelector((state: ApplicationState) => state?.guiStates?.requestedTreeUpdate);

  useEffect(() => {
    getTreeItemsByParent(null);
  }, [user]);

  const selectItem = (selected_marker: TreeMarker | null) => {
    let selected_id = selected_marker?.id ?? null;
    if (selected_id === reduxSelectedId) {
      selected_id = null;
    }
    appDispatch(GuiStore.selectTreeItem(selected_id));
  };

  const handleSelect = (selected: TreeMarker) => () => {
    selectItem(selected);
  };

  const drillDown = (selected_marker: TreeMarker | null) => () => {
    selectItem(null);
    getTreeItemsByParent(selected_marker?.id ?? null);
  };

  const [checked, setChecked] = React.useState<string[]>([]);

  const handleChecked = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selected_id = event.target.id;
    const currentIndex = checked.indexOf(selected_id);
    const newChecked = [...checked];

    if (event.target.checked) {
      newChecked.push(selected_id);
    } else {
      newChecked.splice(currentIndex, 1);
    }

    setChecked(newChecked);
    appDispatch(GuiStore.checkTreeItem(newChecked));
  };

  useEffect(() => {
    getTreeItemsByParent(parent_marker_id ?? null);
  }, [requestTreeUpdate, parent_marker_id]);

  const OnNavigate = (next: boolean) => {
    if (next) {
      appDispatch(TreeStore.fetchByParent({ parent_id:parent_marker_id, start_id:treeStates?.end_id ?? null, end_id:null}));
    } else {
      appDispatch(TreeStore.fetchByParent({ parent_id: parent_marker_id ?? null, start_id:null, end_id:treeStates?.start_id ?? null}));
    }
  };

  return (
    <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
      <TabControl />
      <Box sx={{ flexGrow: 1, backgroundColor: 'lightgray' }}>
        <Toolbar variant="dense">
          <Tooltip title="Go to previous page">
            <IconButton onClick={() => OnNavigate(false)}>
              <ArrowBackIcon />
            </IconButton>
          </Tooltip>
          <Box sx={{ flexGrow: 1 }} />
          <Tooltip title="Go to next page">
            <IconButton onClick={() => OnNavigate(true)}>
              <ArrowForwardIcon />
            </IconButton>
          </Tooltip>
        </Toolbar>
      </Box>
      <Box sx={{ width: '100%', height: '100%', overflow: 'auto' }}>
        <List dense sx={{ minHeight: '100%', width: "100%" }}>
          {markers?.map((marker) => (
            <ListItem key={marker.id} disablePadding
              secondaryAction={marker.has_children &&
                <IconButton size="small" edge="end" aria-label="drill_down" onClick={drillDown(marker)}>
                  <ChevronRightIcon />
                </IconButton>
              }
            >
              <ListItemButton
                selected={reduxSelectedId === marker.id}
                onClick={handleSelect(marker)}
              >
                <ListItemIcon>
                  <Checkbox
                    size="small"
                    edge="start"
                    checked={checked.includes(marker.id)}
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
