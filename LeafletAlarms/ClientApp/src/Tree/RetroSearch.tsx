import * as React from 'react';
import dayjs, { Dayjs } from 'dayjs';
import TextField from '@mui/material/TextField';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { ApiDefaultPagingNum, ApplicationState } from '../store';
import { Box, Button, IconButton, List, ListItem } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as SearchResultStore from '../store/SearchResultStates';
import { ObjPropsSearchDTO, SearchFilterGUI, SearchFilterDTO, DeepCopy } from '../store/Marker';
import { PropertyFilter } from './PropertyFilter';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import * as GuiStore from '../store/GUIStates';
import ToggleButton from '@mui/material/ToggleButton';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function RetroSearch() {
  const INPUT_FORMAT = "YYYY-MM-DDTHH:mm:ss";

  const dispatch = useDispatch();

  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);
  const tracks = useSelector((state: ApplicationState) => state?.tracksStates?.tracks);
  const routs = useSelector((state: ApplicationState) => state?.tracksStates?.routs);

  function GetCopyOfSearchFilter(): SearchFilterGUI {
    let filter = DeepCopy(searchFilter);
    return filter;
  }

  useEffect(() => {
    if (searchFilter == null) {
      var filter: SearchFilterGUI =
      {
        time_start: dayjs().subtract(1,"day").toISOString(),
        time_end: dayjs().toISOString(),
        property_filter: {
          props: [{ prop_name: "track_name", str_val: "lisa_alert" }]
        },
        search_id: ""
      };
      dispatch<any>(GuiStore.actionCreators.applyFilter(filter));
    }
  }, []);

  function UpdateFilterInRedux(filter: SearchFilterGUI, applyFilter: boolean) {
    filter.applied = applyFilter;
    dispatch<any>(GuiStore.actionCreators.applyFilter(filter));

    if (filter.applied != true) {
      dispatch<any>(SearchResultStore.actionCreators.setEmptyResult());
    }
  }

  const handleChange1 = (newValue: Dayjs | null) => {
    try {
      var filter = GetCopyOfSearchFilter();
      filter.time_start = newValue.toISOString();
      UpdateFilterInRedux(filter, false);
    }
    catch (err)
    {

    }
  };

  const handleChange2 = (newValue: Dayjs | null) => {
    try {
      var filter = GetCopyOfSearchFilter();
      filter.time_end = newValue.toISOString();
      UpdateFilterInRedux(filter, false);
    }
    catch (err) {

    }
  };

  function DoSearchTracks(filterIn: SearchFilterGUI) {
    filterIn.search_id = (new Date()).toISOString();
    UpdateFilterInRedux(filterIn, filterIn.applied == true);

    let filter = DeepCopy(searchFilter);

    var filterDto: SearchFilterDTO = {
      search_id: (new Date()).toISOString(),
      property_filter: filter.property_filter,
      time_start: new Date(filter.time_start),
      time_end: new Date(filter.time_end),
      forward: true,
      count: ApiDefaultPagingNum
    }

    if (filterIn.applied != true) {
      
      dispatch<any>(SearchResultStore.actionCreators.setEmptyResult());
    }
    else {
      dispatch<any>(SearchResultStore.actionCreators.getByFilter(filterDto));
    }
    
  }

  const searchTracks = useCallback(
    () => {
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();
      filter.applied = !filter.applied;
      DoSearchTracks(filter);

    }, [searchFilter]);

  const addProperty = useCallback(
    (e: any) => {
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();
      filter.property_filter.props.push({ prop_name: "test_name", str_val: "test_val" });
      UpdateFilterInRedux(filter, false);
    }, [searchFilter]);

  const setPropsFilter = useCallback(
    (propsFilter: ObjPropsSearchDTO) => {
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();
      filter.property_filter = propsFilter;
      UpdateFilterInRedux(filter, false);
    }, [searchFilter]);

  const OnNavigate = useCallback(
    (next: boolean, e: any) => {

      if (tracks == null  && routs == null) {
        return;
      }
      var filter: SearchFilterGUI = GetCopyOfSearchFilter();      
      filter.sort = 0;

      if (next) {
        const MIN_DATE = new Date('2045-10-05T11:03:21');
        var minDate = MIN_DATE;

        if (tracks != null && tracks.length > 0) {         

          tracks.forEach(function (e: any) {
            var curTs = new Date(e.timestamp);

            if (curTs < minDate) {
              minDate = curTs;
            }
          });
        }

        if (routs != null && routs.length > 0) {
          routs.forEach(function (e: any) {
            var curTs = new Date(e.ts_start);

            if (curTs < minDate) {
              minDate = curTs;
            }
          });
        }

        if (minDate == MIN_DATE) {
          minDate = new Date(filter.time_start);//
        }

        var t1 = dayjs(minDate).add(1, 's');
          
        filter.time_start = t1.toISOString();          
        
      }
      else {
        const MAX_DATE = new Date("1945-01-01T00:00:00");

        var maxDate = MAX_DATE;

        if (tracks != null && tracks.length > 0) {
          tracks.forEach(function (e: any) {
            var curTs = new Date(e.timestamp);

            if (curTs > maxDate) {
              maxDate = curTs;
            }
          });
        }

        if (routs != null && routs.length > 0) {
          routs.forEach(function (e: any) {
            var curTs = new Date(e.ts_end);

            if (curTs > maxDate) {
              maxDate = curTs;
            }
          });
        }

        if (maxDate == MAX_DATE) {
          maxDate = new Date(filter.time_end);//
        }

        var t1 = dayjs(maxDate).subtract(1, 's');
        filter.time_end = t1.toISOString();
        filter.sort = -1;
      }

      DoSearchTracks(filter);

    }, [searchFilter, tracks, routs])


  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Box display="flex"
        justifyContent="flex-start"
      >
        <ToggleButton
          color="success"
          value="check"
          aria-label="search"
          selected={searchFilter?.applied == true}
          size="small"
          onChange={() => searchTracks()}>
          <SearchIcon/>
        </ToggleButton>
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
          <Button color="secondary" onClick={(e: any) => OnNavigate(true, e)}>{'>'}</Button>
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
          <Button color="secondary" onClick={(e: any) => OnNavigate(false, e)}>{'<'}</Button>
        </ListItem>

          <Box display="flex"
            justifyContent="flex-start"
        >
          <IconButton color="primary" aria-label="addProp" size="medium" onClick={(e: any) => addProperty(e)}>
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

