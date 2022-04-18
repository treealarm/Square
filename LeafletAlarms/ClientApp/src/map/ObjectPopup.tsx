import { TextField } from '@mui/material';
import * as React from 'react';
import { useCallback } from 'react';
import {
  Popup
} from 'react-leaflet'
import { Marker } from '../store/Marker';

export function ObjectPopup(props: any) {

  const curName = React.useState(props?.marker.name);

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
    if (curName != props.marker.name && curName != null) {
      var updatedMarker: Marker = props.marker;
      updatedMarker.name = curName[0];
      props.updateBaseMarker(updatedMarker);
    }
  };

  return (
    <React.Fragment>
      <Popup onClose={handleOnClose} >
        <table>
          <tbody>
            <tr><td>
              <TextField size="small" fullWidth sx={{ width: '25ch' }} id="outlined" label='Name' defaultValue={props.marker.name}
              onChange={handleChange}              />
            </td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e) => props.deleteMe(props?.marker, e)}>Delete object</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}