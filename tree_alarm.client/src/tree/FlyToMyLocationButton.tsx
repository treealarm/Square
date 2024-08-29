
import { useCallback } from 'react';

import { IconButton, Tooltip } from '@mui/material';
import LocationSearchingIcon from '@mui/icons-material/LocationSearching';

import {
  ViewOption
} from '../store/Marker';
import * as GuiStore from '../store/GUIStates';
import { useAppDispatch } from '../store/configureStore';


export function FlyToMyLocationButton() {

  const appDispatch = useAppDispatch();
  const searchMeOnMap = useCallback(
    () => {

      var viewOption: ViewOption = {
        map_center: null,
        zoom: null,
        find_current_pos: true
      };

      appDispatch<any>(GuiStore.actionCreators.setMapOption(viewOption));
    },[appDispatch]);

  return (
    <Tooltip title={"Search me on Map"}>
      <IconButton aria-label="search" size="medium" onClick={() => searchMeOnMap()}>
        <LocationSearchingIcon fontSize="inherit" />
      </IconButton>
    </Tooltip>
  );
}