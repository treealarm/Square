import * as React from 'react';

import {useCallback, useRef } from 'react';
import { useSelector } from "react-redux";

import * as DiagramTypeStore from '../store/DiagramTypeStates'
import { ApplicationState } from '../store';
import { DeepCopy, IDiagramTypeDTO, IGetDiagramTypesByFilterDTO } from '../store/Marker';

import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import IconButton from '@mui/material/IconButton';
import { Box, Toolbar, Tooltip, TextField, Divider } from '@mui/material';

import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useAppDispatch } from '../store/configureStore';


export function DiagramTypeSearcher() {

  const appDispatch = useAppDispatch();
  const diagramtypes: IDiagramTypeDTO[] = useSelector((state: ApplicationState) => state?.diagramtypeStates?.diagramtypes);
  const diagramType = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);
  const localFilter = useSelector((state: ApplicationState) => state?.diagramtypeStates?.localFilter);

  const handleSelect = useCallback((selected: IDiagramTypeDTO) => () => {
    
    var copy = DeepCopy(selected);
    appDispatch(DiagramTypeStore.set_local_diagram(copy));    
  }, [appDispatch]);

  const countOnPage = 200;

  const timeoutIdRef = useRef<any>(null);


  React.useEffect(() => {
    if (timeoutIdRef.current != null) {
      clearTimeout(timeoutIdRef.current);
    }
      
    timeoutIdRef.current = setTimeout(() => {
      var filterToApplay: IGetDiagramTypesByFilterDTO =
      {
        filter: localFilter,
        forward: true,
        start_id: null,
        count: countOnPage
      };

      appDispatch(DiagramTypeStore.fetchGetDiagramTypesByFilter(filterToApplay));
    }, 1500);
    return () => clearTimeout(timeoutIdRef.current);
  }, [appDispatch, localFilter]);

  function handleChangeName(e: any) {
    const { target: { value } } = e;
    appDispatch(DiagramTypeStore.set_local_filter(value));
  }

  const OnNavigate = useCallback(
    (next: boolean) => {

      var filterToApplay: IGetDiagramTypesByFilterDTO =
      {
        filter: localFilter,
        forward: next,
        start_id: null,
        count: countOnPage
      };
      
      if (next) {
        if (diagramtypes != null && diagramtypes.length > 0) {
          const lastElement = diagramtypes[diagramtypes.length - 1];
          filterToApplay.start_id = lastElement.id;
        }        
      }
      else {
        if (diagramtypes != null && diagramtypes.length > 0) {
          const lastElement = diagramtypes[0];
          filterToApplay.start_id = lastElement.id;
        } 
      }
      appDispatch(DiagramTypeStore.fetchGetDiagramTypesByFilter(filterToApplay));
    }, [appDispatch, diagramtypes, localFilter])

  return (
    <Box sx={{
      width: '100%',
      height: '100%',
      display: 'flex',
      flexDirection: 'column'
    }}>
      <Box sx={{ flexGrow: 1}}>
        <Toolbar variant="dense" >

          <Tooltip title="Go to previous page">
            <IconButton onClick={() => OnNavigate(false)}>
              <ArrowBackIcon />
            </IconButton>
          </Tooltip>
          <Divider><br /></Divider>
          <Tooltip title="Filter">
          
            <TextField size="small"
              sx={{ margin: 3 }}
              fullWidth
              id="filter_name" label='Filter'
              value={localFilter ? localFilter :""}
              onChange={handleChangeName} />
          </Tooltip>
          <Box sx={{ flexGrow: 1 }} />

          <Tooltip title="Go to next page">
            <IconButton onClick={() => OnNavigate(true)}>
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
                <Tooltip title={dType.id}>
                <ListItemButton
                  selected={diagramType?.id == dType?.id} role={undefined}
                  onClick={handleSelect(dType)}>
                  <ListItemText
                    id={dType.id}
                    primary={dType.name}
                  />
                  </ListItemButton>
                </Tooltip>
              </ListItem>
            )}
        </List>
      </Box>
    </Box>
  );
}