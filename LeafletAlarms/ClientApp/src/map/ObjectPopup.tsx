import { Alert, AlertTitle, Box, List, ListItem, ListItemButton, ListItemText } from '@mui/material';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import * as React from 'react';
import { useCallback } from 'react';
import {
  Popup
} from 'react-leaflet'
import { Marker } from '../store/Marker';

const theme = createTheme({
  typography: {
    // In Chinese and Japanese the characters are usually larger,
    // so a smaller fontsize may be appropriate.
    fontSize: 12,
  },
});

export function ObjectPopup(props: any) {

  const curName = React.useState(props.marker.name);

  const handleChange = useCallback(
    (e: any) => {
    const { target: { name, value } } = e;
      curName[0] = value;
    },[]
  );

  function handleOnClose()
  {
    if (props.updateBaseMarker == null) {
      return;
    }
    if (curName[0] != props.marker.name && curName != null) {
      var updatedMarker: Marker = props.marker;
      updatedMarker.name = curName[0];
      props.updateBaseMarker(updatedMarker);
    }
  };

  return (
    <React.Fragment>
      <Popup onClose={handleOnClose}>
        <ThemeProvider theme={theme}>
        <Box>            
           <Alert severity="info">
            <AlertTitle>Name:<strong>{props?.marker?.name}</strong></AlertTitle>
              {props?.marker?.type}/{props?.marker?.id}
            </Alert>
          <List>
            <ListItem disablePadding>
              <ListItemButton onClick={(e) => props.deleteMe(props?.marker, e)}>
                <ListItemText primary="Delete object" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton onClick={(e) => props.editMe(props?.marker, e)}>
                <ListItemText primary="Edit object" />
              </ListItemButton>
              </ListItem>
              <ListItem disablePadding>
                <ListItemButton onClick={(e) => props.selectMe(props?.marker, e)}>
                  <ListItemText primary="Select object" />
                </ListItemButton>
              </ListItem>
          </List>
        </Box>
      </ThemeProvider>  
      </Popup>
      </React.Fragment>
      
  );
}