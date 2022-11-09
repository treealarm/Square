import * as React from 'react';

import {
  Box, Checkbox, FormControlLabel, FormGroup
} from '@mui/material';
import { useDispatch, useSelector } from 'react-redux';
import { SearchFilterGUI } from '../store/Marker';
import * as GuiStore from '../store/GUIStates';

export default function GlobalLayersOptions() {

  const dispatch = useDispatch();
  const searchFilter = useSelector((state) => state?.guiStates?.searchFilter);

  function GetCopyOfSearchFilter(): SearchFilterGUI {
    let filter = Object.assign({}, searchFilter);
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

    dispatch(GuiStore.actionCreators.applyFilter(filter));
  }, [searchFilter]);

  var checks = [
    { "id": "show_objects", "name": "Objects", "checked": searchFilter?.show_objects },
    { "id": "show_tracks", "name": "Tracks", "checked": searchFilter?.show_tracks },
    { "id": "show_routs", "name": "Routs", "checked": searchFilter?.show_routs }
  ];

  return (
    <Box sx={{ border: 1, borderColor: 'divider' }}>
      <FormGroup row>
        {
          checks.map((item, index) =>
            <FormControlLabel control={
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

