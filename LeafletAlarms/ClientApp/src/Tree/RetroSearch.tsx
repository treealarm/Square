import * as React from 'react';
import dayjs, { Dayjs } from 'dayjs';
import TextField from '@mui/material/TextField';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { ApiDefaultPagingNum, ApplicationState } from '../store';
import { Box, Button, ButtonGroup, FormControlLabel, IconButton, List, ListItem, Switch } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import * as SearchResultStore from '../store/SearchResultStates';
import { ObjPropsSearchDTO, SearchFilterGUI, SearchFilterDTO } from '../store/Marker';
import { PropertyFilter } from './PropertyFilter';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import * as GuiStore from '../store/GUIStates';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function RetroSearch() {
  const INPUT_FORMAT = "YYYY-MM-DDTHH:mm:ss";

  const dispatch = useDispatch();

  const searchFilter = useSelector((state) => state?.guiStates?.searchFilter);
  const tracks = useSelector((state) => state?.tracksStates?.tracks);
  const routs = useSelector((state) => state?.tracksStates?.routs);

  const handleCheckTimeBegin = (event: React.ChangeEvent<HTMLInputElement>) => {
    var filter = GetCopyOfSearchFilter();
    filter.time_start_enabled = event.target.checked;
    UpdateFilterInRedux(filter);
  };

  const handleCheckTimeEnd = (event: React.ChangeEvent<HTMLInputElement>) => {
    var filter = GetCopyOfSearchFilter();
    filter.time_end_enabled = event.target.checked;
    UpdateFilterInRedux(filter);
  };

  

  function GetCopyOfSearchFilter(): SearchFilterGUI {
    let filter = Object.assign({}, searchFilter);
    return filter;
  }

  useEffect(() => {
    if (searchFilter == null) {
      var filter: SearchFilterGUI =
      {
        time_start: new Date(dayjs().subtract(1,"day").toISOString()),
        time_end: new Date(dayjs().toISOString()),
        property_filter: {
          props: [{ prop_name: "track_name", str_val: "lisa_alert" }]
        },
        search_id: "0",
        time_start_enabled: true,
        time_end_enabled: true
      };
      dispatch(GuiStore.actionCreators.applyFilter(filter));
    }
  }, []);

  function UpdateFilterInRedux(filter: SearchFilterGUI) {
    dispatch(GuiStore.actionCreators.applyFilter(filter));    
  }

  const handleChange1 = (newValue: Dayjs | null) => {
    try {
      var filter = GetCopyOfSearchFilter();
      filter.time_start = new Date(newValue.toISOString());
      UpdateFilterInRedux(filter);
    }
    catch (err)
    {

    }
  };

  const handleChange2 = (newValue: Dayjs | null) => {
    try {
      var filter = GetCopyOfSearchFilter();
      filter.time_end = new Date(newValue.toISOString());
      UpdateFilterInRedux(filter);
    }
    catch (err) {

    }
  };

  function DoSearchTracks(filter: SearchFilterGUI) {
    filter.search_id = (new Date()).toISOString();
    UpdateFilterInRedux(filter);

    var filterDto: SearchFilterDTO = {
      search_id: (new Date()).toISOString(),
      property_filter: filter.property_filter,
      time_start: filter.time_start,
      time_end: filter.time_end,
      forward: true,
      count: ApiDefaultPagingNum
    }

    if (!searchFilter?.time_start_enabled) {
      filterDto.time_start = null;
    }
    if (!searchFilter?.time_end_enabled) {
      filterDto.time_end = null;
    }
    dispatch(SearchResultStore.actionCreators.getByFilter(filterDto));
  }

  const searchTracks = useCallback(
    (e) => {
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();

      DoSearchTracks(filter);

    }, [searchFilter]);

  const addProperty = useCallback(
    (e) => {
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();
      filter.property_filter.props.push({ prop_name: "test_name", str_val: "test_val" });
      UpdateFilterInRedux(filter);
    }, [searchFilter]);

  const setPropsFilter = useCallback(
    (propsFilter: ObjPropsSearchDTO) => {
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();
      filter.property_filter = propsFilter;
      UpdateFilterInRedux(filter);
    }, [searchFilter]);

  const OnNavigate = useCallback(
    (next: boolean, e) => {

      if ((tracks == null
        || tracks.length == 0)
        && (routs == null
        || routs.length == 0)) {
        return;
      }
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();      

      if (next) {
        var minDate = new Date('2045-10-05T11:03:21');

        if (tracks != null && tracks.length > 0) {         

          tracks.forEach(function (e) {
            var curTs = new Date(e.timestamp);

            if (curTs < minDate) {
              minDate = curTs;
            }
          });
        }

        if (routs != null && routs.length > 0) {
          routs.forEach(function (e) {
            var curTs = new Date(e.ts_start);

            if (curTs < minDate) {
              minDate = curTs;
            }
          });
        }

        var t1 = dayjs(minDate).add(1, 's');
          
        filter.time_start = new Date(t1.toISOString());          
        
      }
      else {
        var maxDate = new Date("1945-01-01T00:00:00");

        if (tracks != null && tracks.length > 0) {
          tracks.forEach(function (e) {
            var curTs = new Date(e.timestamp);

            if (curTs > maxDate) {
              maxDate = curTs;
            }
          });
        }

        if (routs != null && routs.length > 0) {
          routs.forEach(function (e) {
            var curTs = new Date(e.ts_end);

            if (curTs > maxDate) {
              maxDate = curTs;
            }
          });
        }

        var t1 = dayjs(maxDate).subtract(1, 's');
        filter.time_end = new Date(t1.toISOString())
      }

      DoSearchTracks(filter);

    }, [searchFilter, tracks, routs])


  return (
      <LocalizationProvider dateAdapter={AdapterDayjs}>
        <List>
          <ListItem>
            <ButtonGroup variant="contained" aria-label="search button group">
              <IconButton aria-label="search" size="large" onClick={(e) => searchTracks(e)}>
                <SearchIcon fontSize="inherit" />
              </IconButton>
              <IconButton aria-label="addProp" size="large" onClick={(e) => addProperty(e)}>
                <LibraryAddIcon fontSize="inherit" />
            </IconButton>
            <Button onClick={(e) => OnNavigate(false, e)}>{'<'}</Button>
            <Button onClick={(e) => OnNavigate(true, e)}>{'>'}</Button>
            </ButtonGroup>
          </ListItem>
          <ListItem>
          <DateTimePicker
            label="begin (ISO 8601)"
            value={searchFilter?.time_start}
            onChange={handleChange1}
            inputFormat={INPUT_FORMAT}
            disabled={!searchFilter?.time_start_enabled}
            renderInput={(params) =>
              <TextField {...params}
                inputProps={
                  {
                    ...params.inputProps,
                    placeholder: INPUT_FORMAT
                  } 
                }/>}
          />
            <Switch size="small"
            checked={searchFilter == null ? true : searchFilter.time_start_enabled}
              onChange={handleCheckTimeBegin} />


          </ListItem>
          <ListItem>
          <DateTimePicker
            label="end (ISO 8601)"
            value={searchFilter?.time_end}
            onChange={handleChange2}
            inputFormat={INPUT_FORMAT}
            disabled={!searchFilter?.time_end_enabled}
            renderInput={(params) =>
              <TextField {...params}
                inputProps={
                  {
                    ...params.inputProps,
                    placeholder: INPUT_FORMAT
                  }
                } />}
          />

            <Switch size="small"
            checked={ searchFilter == null ? true : searchFilter.time_end_enabled}
              onChange={handleCheckTimeEnd} />
      
          </ListItem>
        <ListItem>
          <PropertyFilter
            propsFilter={searchFilter?.property_filter}
            setPropsFilter={setPropsFilter} />
          </ListItem>
        </List>
      </LocalizationProvider>
  );
}

