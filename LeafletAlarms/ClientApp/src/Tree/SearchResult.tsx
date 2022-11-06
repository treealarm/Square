import * as React from 'react';

import { useEffect, useCallback, useState } from 'react';
import { useDispatch, useSelector, useStore } from "react-redux";

import * as SearchResultStore from '../store/SearchResultStates';
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
import { Box, Button, ButtonGroup } from '@mui/material';


declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function SearchResult() {

  const dispatch = useDispatch();

  useEffect(() => {
    console.log('ComponentDidMount SearchResult');
  }, []);

  const searchStates = useSelector((state) => state?.searchResultStates);

  const markers = searchStates.list;

  // Selected.
  const reduxSelectedId = useSelector((state) => state?.guiStates?.selected_id);

  function selectItem(selected_marker: TreeMarker | null) {

    var selected_id = selected_marker?.id;

    if (selected_id == reduxSelectedId) {
      selected_id = null;
    }

    dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
  }

  const handleSelect = useCallback((selected: TreeMarker) => () => {
    selectItem(selected);
  }, [reduxSelectedId]);

  const OnNavigate = useCallback(
    (next: boolean, e) => {

      let filter = Object.assign({}, searchStates.filter);
      filter.forward = next;
      filter.start_id = null;

      if (next) {
        if (searchStates.list.length > 0) {          
          filter.start_id = searchStates.list[searchStates.list.length - 1].id;
        }
        
        dispatch(          
          SearchResultStore.actionCreators.getByFilter(filter)
        );
      }
      else {
        if (searchStates.list.length > 0) {
          filter.start_id = searchStates.list[0].id;
        }
        
        dispatch(
          SearchResultStore.actionCreators.getByFilter(filter)
        );
      }
    }, [searchStates?.list])

  return (
    <Box sx={{
      width: '100%',
      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List>
        <ListItem>
          <ButtonGroup variant="contained" aria-label="navigation button group">
            <Button onClick={(e) => OnNavigate(false, e)}>{'<'}</Button>
            <Button onClick={(e) => OnNavigate(true, e)}>{'>'}</Button>
          </ButtonGroup>
        </ListItem>
        {
          markers?.map((marker, index) =>
            <ListItem
              key={marker.id}
              disablePadding
            >
              <ListItemButton selected={reduxSelectedId == marker.id} role={undefined} onClick={handleSelect(marker)}>
                <ListItemText id={marker.id} primary={marker.name} />
              </ListItemButton>
            </ListItem>
          )}
      </List>
    </Box>
  );
}