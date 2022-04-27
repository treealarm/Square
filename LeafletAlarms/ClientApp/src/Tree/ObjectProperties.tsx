import * as React from 'react';

import { useEffect, useCallback, useState } from 'react';
import { useDispatch, useSelector, useStore } from "react-redux";

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
import { Box } from '@mui/material';


declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectProperties() {

  const dispatch = useDispatch();
  const selected_id = useSelector((state) => state?.guiStates?.selected_id);

  return (
    <Box sx={{
      width: '100%',
      maxWidth: 460,
      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List>
        <ListItem>{selected_id}</ListItem>
        <ListItem>{selected_id}</ListItem>
      </List>
    </Box>
  );
}