import * as React from 'react';

import {
  Box, Checkbox, FormControlLabel, FormGroup
} from '@mui/material';
import { useDispatch, useSelector } from 'react-redux';
import { DeepCopy, SearchFilterGUI } from '../store/Marker';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';

export default function GlobalLayersOptions() {

  const dispatch = useDispatch();
  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  function GetCopyOfSearchFilter(): SearchFilterGUI {
    let filter = DeepCopy(searchFilter);
    return filter;
  }

  // Checked.
  const handleChecked = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {

    var selected_id = event.target.id;

    var filter = GetCopyOfSearchFilter();

    if (selected_id == "show_objects") {
      filter.show_objects = event.target.checked;
    }
    if (selected_id == "show_tracks") {
      filter.show_tracks = event.target.checked;
    }
    if (selected_id == "show_routs") {
      filter.show_routs = event.target.checked;
    }

    dispatch<any>(GuiStore.actionCreators.applyFilter(filter));
  }, [searchFilter]);

  var checks = [
    { "id": "show_objects", "name": "Objects", "checked": searchFilter?.show_objects },
    { "id": "show_tracks", "name": "Tracks", "checked": searchFilter?.show_tracks },
    { "id": "show_routs", "name": "Routs", "checked": searchFilter?.show_routs }
  ];

  return (
    <Box>
      <FormGroup row>
        {
          checks.map((item, index) =>
            <FormControlLabel key={"fkl"+item.id} control={
              <Checkbox
                checked={item.checked != false}
                id={item.id}
                onChange={handleChecked}
                size="small"
                tabIndex={-1}
                disableRipple
              />
            }
              label={item.name}
            />
          )}
      </FormGroup>
    </Box>
  );
}

