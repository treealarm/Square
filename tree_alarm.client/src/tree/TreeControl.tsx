import * as React from 'react';

import { useEffect, useCallback, useState} from 'react';
import { useDispatch, useSelector, useStore } from "react-redux";

import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { TreeMarker, ViewOption } from '../store/Marker';

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

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function TreeControl() {

  const dispatch = useDispatch();

  function getTreeItemsByParent(parent_marker_id: string | null)
  {
    dispatch<any>(
      TreeStore.actionCreators.getByParent(parent_marker_id, null, null)
    );
  }

  const treeStates = useSelector((state: ApplicationState) => state?.treeStates);

  const markers = useSelector((state: ApplicationState) => state?.treeStates?.children);
  const parent_marker_id = useSelector((state: ApplicationState) => state?.treeStates?.parent_id);
  const user = useSelector((state: ApplicationState) => state?.rightsStates?.user);

  // Selected.
  const reduxSelectedId = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  useEffect(() => {
    console.log('ComponentDidMount TreeControl');
    getTreeItemsByParent(null);
  }, [user]);


  function selectItem(selected_marker: TreeMarker | null) {

    var selected_id = selected_marker?.id;

    if (selected_id == reduxSelectedId) {
      selected_id = null;
    }
    
    dispatch<any>(GuiStore.actionCreators.selectTreeItem(selected_id));
  }

  const handleSelect = useCallback((selected: TreeMarker) => () => {
    selectItem(selected);
  }, [reduxSelectedId]);

    

  // Drill down.
  const drillDown = useCallback((selected_marker: TreeMarker|null) => () => {
    selectItem(null);
    getTreeItemsByParent(selected_marker?.id);
  }, []);

  // Checked.
  const [checked, setChecked] = React.useState([]);
  const handleChecked = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {

    var selected_id = event.target.id;
    const currentIndex = checked.indexOf(selected_id);
    const newChecked = [...checked];

    if (event.target.checked) {
      newChecked.push(selected_id);
    } else {
      newChecked.splice(currentIndex, 1);
    }

    setChecked(newChecked);

    dispatch<any>(GuiStore.actionCreators.checkTreeItem(newChecked));
  }, [checked]);

  const requestTreeUpdate = useSelector((state: ApplicationState) => state?.guiStates?.requestedTreeUpdate);

  useEffect(() => {
    getTreeItemsByParent(parent_marker_id);
  }, [requestTreeUpdate, parent_marker_id]);

  const OnNavigate = useCallback(
    (next: boolean, e: any) => {
      if (next) {
        dispatch<any>(
          TreeStore.actionCreators.getByParent(parent_marker_id, treeStates.end_id, null)
        );
      }
      else {
        dispatch<any>(
          TreeStore.actionCreators.getByParent(parent_marker_id, null, treeStates.start_id)
        );
      }
    }, [treeStates])

    return (
      <Box sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column'
      }}>
        <TabControl />
        <Box sx={{ flexGrow: 1, backgroundColor: 'lightgray' }}>
          <Toolbar variant="dense">

            <Tooltip title="Go to previous page">
            <IconButton onClick={(e: any) => OnNavigate(false, e)}>
              <ArrowBackIcon />
              </IconButton>
            </Tooltip>

            <Box sx={{ flexGrow: 1 }} />

            <Tooltip title="Go to next page">
            <IconButton onClick={(e: any) => OnNavigate(true, e)}>
              <ArrowForwardIcon />
              </IconButton>
            </Tooltip>

          </Toolbar>
        </Box>

        <Box sx={{
          width: '100%',
          height: '100%',
          overflow: 'auto'
        }}>
        <List dense sx={{
          minHeight: '100%', width: "100%" }}>
         {
          markers?.map((marker:any) =>
            <ListItem
              key={marker.id}
              disablePadding
              selected={reduxSelectedId == marker.id}
              secondaryAction={
                marker.has_children &&
                <IconButton size="small" edge="end" aria-label="drill_down" onClick={drillDown(marker)}>
                  <ChevronRightIcon />
                </IconButton>
              }
            >
              <ListItemButton
                selected={reduxSelectedId == marker.id} role={undefined}
                onClick={handleSelect(marker)}>
                <ListItemIcon>
                  <Checkbox
                    size="small"
                    edge="start"
                    checked={checked.indexOf(marker.id) !== -1}
                    tabIndex={-1}
                    disableRipple
                    id={marker.id}
                    onChange={handleChecked}
                  />
                </ListItemIcon>
                <ListItemText
                  id={marker.id}
                  primary={marker.name}                  
                />
              </ListItemButton>
          </ListItem>
        )}
          </List>
        </Box>
        </Box>
    );
}