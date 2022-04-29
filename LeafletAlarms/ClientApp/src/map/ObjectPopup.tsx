import { TextField } from '@mui/material';
import * as React from 'react';
import { useCallback } from 'react';
import {
  Popup
} from 'react-leaflet'
import { Marker } from '../store/Marker';

export function ObjectPopup(props: any) {

  const curName = React.useState(props?.marker?.name);

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
      <Popup onClose={handleOnClose} >
        <table>
          <tbody>
            <tr className="menu_header"><td>
              <span>{props?.marker?.type}/{props?.marker?.id}</span>
            </td></tr>
            <tr className="menu_header"><td>
              <span>Name:{props?.marker?.name}</span>
            </td></tr>
            <tr className="border-bottom"><td>
              <span className="menu_item" onClick={(e) => props.deleteMe(props?.marker, e)}>Delete object</span>
            </td></tr>
            <tr className="border-bottom"><td>
              <span className="menu_item" onClick={(e) => props.editMe(props?.marker, e)}>Edit object</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}