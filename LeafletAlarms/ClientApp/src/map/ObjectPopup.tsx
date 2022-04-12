import * as React from 'react';
import {
  Popup
} from 'react-leaflet'

export function ObjectPopup(props) {
  return (
    <React.Fragment>
      <Popup>
        <table>
          <tbody>
            <tr><td>{props?.marker?.name}</td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e) => props.deleteMe(props?.marker, e)}>Delete object</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}