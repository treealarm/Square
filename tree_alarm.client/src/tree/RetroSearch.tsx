import * as React from 'react';
import dayjs, { Dayjs } from 'dayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { ApplicationState } from '../store';
import {
  Box, IconButton, List, ListItem, Tooltip
} from '@mui/material';

import { useCallback, useEffect } from 'react';
import { useSelector } from "react-redux";
import * as SearchResultStore from '../store/SearchResultStates';
import { ObjPropsSearchDTO, SearchFilterGUI, SearchFilterDTO, DeepCopy } from '../store/Marker';
import { PropertyFilter } from './PropertyFilter';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import * as GuiStore from '../store/GUIStates';

import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useAppDispatch } from '../store/configureStore';
import { SearchApplyButton } from './SearchApplyButton';
import { ApiDefaultPagingNum, INPUT_DATETIME_FORMAT } from '../store/constants';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function RetroSearch() {
  const dispatch = useAppDispatch();

  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);
  const tracks = useSelector((state: ApplicationState) => state?.tracksStates?.tracks);
  const routes = useSelector((state: ApplicationState) => state?.tracksStates?.routes);

  function GetCopyOfSearchFilter(): SearchFilterGUI {
    let filter = DeepCopy(searchFilter);
    return filter;
  }

  useEffect(() => {
    if (searchFilter == null) {
      console.error("Normally search filter should be inited");
    }
  }, []);

  useEffect(() => {
    var filter: SearchFilterGUI = GetCopyOfSearchFilter();

    if (filter == null) {
      return;
    }
    DoSearchTracks(filter);
  }, [searchFilter?.applied]);

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

    if (filterIn == null) {
      console.error("DoSearchTracks:", "filterIn == null");
      return;
    }

    filterIn.search_id = (new Date()).toISOString();
    UpdateFilterInRedux(filterIn, filterIn.applied == true);

    let filter = DeepCopy(searchFilter);

    var filterDto: SearchFilterDTO = {
      search_id: (new Date()).toISOString(),
      property_filter: filter.property_filter,
      time_start: filter.time_start,
      time_end: filter.time_end,
      forward: 1,
      count: ApiDefaultPagingNum
    }

    if (filterIn.applied != true) {      
      dispatch<any>(SearchResultStore.actionCreators.setEmptyResult());
    }
    else {
      dispatch<any>(SearchResultStore.actionCreators.getByFilter(filterDto));
    }    
  }

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

      if (tracks == null  && routes == null) {
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

        if (routes != null && routes.length > 0) {
          routes.forEach(function (e: any) {
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

        if (routes != null && routes.length > 0) {
          routes.forEach(function (e: any) {
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

    }, [searchFilter, tracks, routes])


  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <List dense>
        <ListItem>
          <Box display="flex" justifyContent="flex-start">
            <SearchApplyButton hideIfNotPushed={false} />
          </Box>
        </ListItem>
        <ListItem>
          <DateTimePicker
            label="begin"
            value={dayjs(searchFilter?.time_start)}
            onChange={handleChange1}
            views={['year', 'month', 'day', 'hours', 'minutes', 'seconds']}
            format={INPUT_DATETIME_FORMAT}
          />

          <Tooltip title={"Shift begin time to next track"}>
          <IconButton onClick={(e: any) => OnNavigate(true, e)}>
            <ArrowForwardIcon />
            </IconButton>
          </Tooltip>

          </ListItem>
          <ListItem>
          <DateTimePicker
            label="end"
            value={dayjs(searchFilter?.time_end)}
            onChange={handleChange2}
            format={INPUT_DATETIME_FORMAT}
          />

          <Tooltip title={"Shift begin time to previous track"}>
          <IconButton onClick={(e: any) => OnNavigate(false, e)}>
            <ArrowBackIcon />
            </IconButton>
          </Tooltip>

        </ListItem>

          <Box display="flex"
          justifyContent="flex-start">
          <Tooltip title={"Add property value into the filter"}>
          <IconButton aria-label="addProp" size="medium" onClick={(e: any) => addProperty(e)}>
                <LibraryAddIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>
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

