import * as React from 'react';

import { useEffect, useCallback, useState } from 'react';
import { useDispatch, useSelector, useStore } from "react-redux";

import * as DiagramTypeStore from '../store/DiagramTypeStates'
import { ApplicationState } from '../store';
import { IGetDiagramTypesByFilterDTO, TreeMarker, ViewOption } from '../store/Marker';

import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { Box, Toolbar, Tooltip, TextField } from '@mui/material';

import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useAppDispatch } from '../store/configureStore';


export function DiagramTypeSearcher() {

  const appDispatch = useAppDispatch();
  const diagramtypes = useSelector((state: ApplicationState) => state?.diagramtypeStates?.diagramtypes);
  const diagramType = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);

  const [filter, set_filter] = React.useState("");

  const handleSelect = useCallback((selected: TreeMarker) => () => {

  }, [diagramtypes]);

  function handleChangeName(e: any) {
    const { target: { id, value } } = e;
    set_filter(value);
    var filterToApplay: IGetDiagramTypesByFilterDTO =
    {
      filter: value,
      forward: true,
      start_id: null
    };
    appDispatch(DiagramTypeStore.fetchGetDiagramTypesByFilter(filterToApplay));
  };

  const OnNavigate = useCallback(
    (next: boolean, e: any) => {
      if (next) {

      }
      else {

      }
    }, [])

  return (
    <Box sx={{
      width: '100%',
      height: '100%',
      display: 'flex',
      flexDirection: 'column'
    }}>
      <Box sx={{ flexGrow: 1, backgroundColor: 'lightgray' }}>
        <Toolbar variant="dense">

          <Tooltip title="Go to previous page">
            <IconButton onClick={(e: any) => OnNavigate(false, e)}>
              <ArrowBackIcon />
            </IconButton>
          </Tooltip>
          <ListItem>
            <TextField size="small"
              fullWidth
              id="filter_name" label='Filter'
              value={filter}
              onChange={handleChangeName} />
          </ListItem>
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
          minHeight: '100%', width: "100%"
        }}>
          {
            diagramtypes?.map((dType: any) =>
              <ListItem
                key={dType.id}
                disablePadding
                selected={diagramType?.id == dType?.id}
              >
                <ListItemButton
                  selected={diagramType?.id == dType?.id} role={undefined}
                  onClick={handleSelect(dType)}>
                  <ListItemText
                    id={dType.id}
                    primary={dType.name}
                  />
                </ListItemButton>
              </ListItem>
            )}
        </List>
      </Box>
    </Box>
  );
}