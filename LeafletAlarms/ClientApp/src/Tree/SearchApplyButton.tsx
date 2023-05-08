
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


export function SearchApplyButton(props: { hideIfNotPushed: boolean }) {
  const dispatch = useAppDispatch();

  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  const toggleApply = useCallback(
    () => {
      let filter: SearchFilterGUI = DeepCopy(searchFilter);      
      filter.applied = !filter.applied;
      filter.search_id = (new Date()).toISOString();
      dispatch<any>(GuiStore.actionCreators.applyFilter(filter));
    }, [searchFilter]);

  if (props.hideIfNotPushed && searchFilter?.applied != true) {
    return null;
  }
  return (
    <Tooltip title={"Search tracks by time and properties and objects by properties"}>
      <ToggleButton
        value="check"
        aria-label="search"
        selected={searchFilter?.applied == true}
        size="small"
        onChange={() => toggleApply()}>
        <FindInPageIcon />
      </ToggleButton>
    </Tooltip>
  );
}