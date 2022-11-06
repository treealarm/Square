import * as React from 'react';
import dayjs, { Dayjs } from 'dayjs';
import TextField from '@mui/material/TextField';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { ApiDefaultPagingNum, ApplicationState } from '../store';
import { Box, Button, ButtonGroup, IconButton, List, ListItem } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from "react-redux";
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
        search_id: ""
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

      if (tracks == null  && routs == null) {
        return;
      }
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();      
      filter.sort = 0;

      if (next) {
        const MIN_DATE = new Date('2045-10-05T11:03:21');
        var minDate = MIN_DATE;

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

        if (minDate == MIN_DATE) {
          minDate = new Date(filter.time_start.toISOString());//
        }

        var t1 = dayjs(minDate).add(1, 's');
          
        filter.time_start = new Date(t1.toISOString());          
        
      }
      else {
        const MAX_DATE = new Date("1945-01-01T00:00:00");

        var maxDate = MAX_DATE;

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

        if (maxDate == MAX_DATE) {
          maxDate = new Date(filter.time_end.toISOString());//
        }

        var t1 = dayjs(maxDate).subtract(1, 's');
        filter.time_end = new Date(t1.toISOString());
        filter.sort = -1;
      }

      DoSearchTracks(filter);

    }, [searchFilter, tracks, routs])


  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Box display="flex"
        justifyContent="flex-start"
      >
        <IconButton aria-label="search" size="medium" onClick={(e) => searchTracks(e)}>
          <SearchIcon fontSize="inherit" />
        </IconButton>
      </Box>

      <List>
          <ListItem>
          <DateTimePicker
            label="begin (ISO 8601)"
            value={searchFilter?.time_start}
            onChange={handleChange1}
            inputFormat={INPUT_FORMAT}
            renderInput={(params) =>
              <TextField {...params}
                inputProps={
                  {
                    ...params.inputProps,
                    placeholder: INPUT_FORMAT
                  } 
                }/>}
          />
          <Button onClick={(e) => OnNavigate(true, e)}>{'>'}</Button>
          </ListItem>
          <ListItem>
          <DateTimePicker
            label="end (ISO 8601)"
            value={searchFilter?.time_end}
            onChange={handleChange2}
            inputFormat={INPUT_FORMAT}
            renderInput={(params) =>
              <TextField {...params}
                inputProps={
                  {
                    ...params.inputProps,
                    placeholder: INPUT_FORMAT
                  }
                } />}
          />
          <Button onClick={(e) => OnNavigate(false, e)}>{'<'}</Button>
        </ListItem>

          <Box display="flex"
            justifyContent="flex-start"
          >
              <IconButton aria-label="addProp" size="medium" onClick={(e) => addProperty(e)}>
                <LibraryAddIcon fontSize="inherit" />
              </IconButton>            
          </Box>

        <ListItem>
          <PropertyFilter
            propsFilter={searchFilter?.property_filter}
            setPropsFilter={setPropsFilter} />
          </ListItem>
        </List>
      </LocalizationProvider>
  );
}

