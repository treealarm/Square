import * as React from 'react';
import { ApplicationState } from '../store';
import { List, ListItem, ListItemButton, ListItemText, TextField, Typography } from '@mui/material';
import { useSelector } from 'react-redux';
import { DateTimeField, LocalizationProvider } from '@mui/x-date-pickers';
import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import * as GuiStore from '../store/GUIStates';
import { SearchMeOnMap } from './SearchMeOnMap';
import dayjs from 'dayjs';

const INPUT_FORMAT = "YYYY-MM-DD HH:mm:ss";

export function TrackProps() {

  const appDispatch = useAppDispatch();
  const selected_track = useSelector((state: ApplicationState) => state?.tracksStates?.selected_track);

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const handleSelect = useCallback(() => () => {

    if (selected_id == null) {
      appDispatch<any>(GuiStore.actionCreators.selectTreeItem(selected_track?.figure.id));
    }
    else {
      appDispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
    }

  }, [selected_id, selected_track]);

  if (selected_track == null) {
    return null;
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <List>
        <ListItem key="figure_id">

          <ListItemButton
            selected={selected_id == selected_track?.figure.id}
            onClick={handleSelect()}>
            <ListItemText id={selected_track?.figure.id}
              primary={selected_track?.figure.id}
            />
          </ListItemButton>

        </ListItem>

        <ListItem key="track_id">
          <SearchMeOnMap
            geometry={selected_track?.figure?.location}
            text={selected_track?.id}
            zoom_min={null} />
        </ListItem>

        <ListItem key="timestamp">
          <DateTimeField
            size="small"
            readOnly
            label="timestamp"
            value={dayjs(selected_track?.timestamp)}
            format={INPUT_FORMAT}
          />
        </ListItem>
        {
          selected_track?.extra_props.map((prop, index) =>
            <ListItem key={index} >
              <TextField
                size="small"
                label={prop.prop_name} value={prop.str_val}>
                id={prop?.prop_name + prop?.str_val}
              </TextField>
            </ListItem>
          )}
      </List>
    </LocalizationProvider>
  );
}
