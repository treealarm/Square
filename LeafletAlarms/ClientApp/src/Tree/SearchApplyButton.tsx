
import * as React from 'react';
import {  ApplicationState } from '../store';
import {Tooltip} from '@mui/material';
import FindInPageIcon from '@mui/icons-material/FindInPage';
import { useCallback } from 'react';
import { useSelector } from "react-redux";
import { DeepCopy, SearchFilterGUI } from '../store/Marker';
import * as GuiStore from '../store/GUIStates';
import ToggleButton from '@mui/material/ToggleButton';
import { useAppDispatch } from '../store/configureStore';


export function SearchApplyButton() {
  const dispatch = useAppDispatch();

  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  const searchTracks = useCallback(
    () => {
      let filter: SearchFilterGUI = DeepCopy(searchFilter);
      filter.applied = !filter.applied;
      dispatch<any>(GuiStore.actionCreators.applyFilter(filter));
    }, [searchFilter]);

  return (
    <Tooltip title={"Search tracks by time and properties and objects by properties"}>
      <ToggleButton
        value="check"
        aria-label="search"
        selected={searchFilter?.applied == true}
        size="small"
        onChange={() => searchTracks()}>
        <FindInPageIcon />
      </ToggleButton>
    </Tooltip>
  );
}