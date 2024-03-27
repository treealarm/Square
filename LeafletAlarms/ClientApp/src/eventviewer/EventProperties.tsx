import * as React from 'react';

import { useEffect } from 'react';
import { useSelector } from "react-redux";
import * as EventsStore from '../store/EventsStates'

import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, TextField } from '@mui/material';

import { useAppDispatch } from '../store/configureStore';
import { IEventDTO } from '../store/Marker';


export function EventProperties() {

  const appDispatch = useAppDispatch();
  const selected_event: IEventDTO = useSelector((state: ApplicationState) => state?.eventsStates?.selected_event);

  useEffect(() => {

  }, []);
  
  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 0
    }}>

      <List dense>
        {
          selected_event?.extra_props?.map((item, index) =>
            <ListItem key={index}>

              <TextField size="small"
                fullWidth
                id={item.prop_name} label={item.prop_name}
                value={item.str_val}
                inputProps={{ readOnly: true }}/>
            </ListItem>
          )
        }
      </List>
    </Box>
  );
}