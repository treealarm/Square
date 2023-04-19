import * as React from 'react';

import * as SearchResultStore from '../store/SearchResultStates';
import { ApplicationState } from '../store';
import { DeepCopy, ITrackPointDTO} from '../store/Marker';
import * as TracksStore from '../store/TracksStates';

import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import { Box, Button, ButtonGroup } from '@mui/material';
import { useAppDispatch } from '../store/configureStore';
import { useSelector } from 'react-redux';
import { useCallback } from 'react';


declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function SearchResult() {

  const appDispatch = useAppDispatch();

  const searchStates = useSelector((state: ApplicationState) => state?.searchResultStates);

  const markers = searchStates.list;

  // Selected.
  const reduxSelectedTrack = useSelector((state: ApplicationState) => state?.tracksStates?.selected_track);


  const handleSelect = useCallback((selected_marker: ITrackPointDTO) => () => {
    var selected_track_id = selected_marker?.id;

    if (selected_track_id == reduxSelectedTrack?.id) {
      selected_marker = null;
    }
    console.log("SELECTED_TRACK:", selected_track_id, " ", reduxSelectedTrack);
    appDispatch<any>(TracksStore.actionCreators.OnSelectTrack(selected_marker));

  }, [reduxSelectedTrack]);

  const OnNavigate = useCallback(
    (next: boolean, e: any) => {

      let filter = DeepCopy(searchStates.filter);
      filter.forward = next;
      filter.start_id = null;

      if (next) {
        if (searchStates.list.length > 0) {          
          filter.start_id = searchStates.list[searchStates.list.length - 1].id;
        }
        
        appDispatch<any>(          
          SearchResultStore.actionCreators.getByFilter(filter)
        );
      }
      else {
        if (searchStates.list.length > 0) {
          filter.start_id = searchStates.list[0].id;
        }
        
        appDispatch<any>(
          SearchResultStore.actionCreators.getByFilter(filter)
        );
      }
    }, [searchStates?.list])

  function getLocalTimeString(timestamp: string) {
    try {
      var d = new Date(timestamp);
      return d.toLocaleDateString() + " " + d.toLocaleTimeString()
    }
    catch (e:any) {
      return (e as Error)?.message;
    }
  }
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
            <Button onClick={(e: any) => OnNavigate(false, e)}>{'<'}</Button>
            <Button onClick={(e: any) => OnNavigate(true, e)}>{'>'}</Button>
          </ButtonGroup>
        </ListItem>
        {
          markers?.map((marker, index) =>
            <ListItem
              key={marker.id}
              disablePadding
            >
              <ListItemButton
                selected={reduxSelectedTrack?.id == marker.id}
                role={undefined}
                onClick={handleSelect(marker)}>
                <ListItemText id={marker.id}
                  primary={getLocalTimeString(marker.timestamp)}
                  secondary={marker.timestamp} />
              </ListItemButton>
            </ListItem>
          )}
      </List>
    </Box>
  );
}