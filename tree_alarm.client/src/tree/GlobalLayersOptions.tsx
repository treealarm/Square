import * as React from 'react';

import {
  Box,
  Checkbox,
  FormControlLabel,
  FormGroup
} from '@mui/material';
import { useDispatch, useSelector } from 'react-redux';
import { DeepCopy, SearchFilterGUI } from '../store/Marker';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import dayjs from 'dayjs';


export default function GlobalLayersOptions() {

  const dispatch = useDispatch();
  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  const GetCopyOfSearchFilter = React.useCallback((): SearchFilterGUI=> {
    let filter = DeepCopy(searchFilter);

    if (filter == null) {
      filter =
      {
        time_start: dayjs().subtract(1, "day").toISOString(),
        time_end: dayjs().toISOString(),
        property_filter: {
          props: [{ prop_name: "track_name", str_val: "mentovskoy_bobik" }]
        },
        search_id: "",
        show_objects: true,
        show_routes: true,
        show_tracks: true
      };
    }
    return filter;
  },[searchFilter])

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
    if (selected_id == "show_routes") {
      filter.show_routes = event.target.checked;
    }

    dispatch(GuiStore.applyFilter(filter));
  }, [GetCopyOfSearchFilter, dispatch]);

  var checks = [
    { "id": "show_objects", "name": "Objects", "checked": searchFilter?.show_objects },
    { "id": "show_tracks", "name": "Tracks", "checked": searchFilter?.show_tracks },
    { "id": "show_routes", "name": "Routes", "checked": searchFilter?.show_routes }
  ];

  return (
    <Box>
      <FormGroup row>
        {
          checks.map((item) =>
            <FormControlLabel key={"fkl" + item.id} control={
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

