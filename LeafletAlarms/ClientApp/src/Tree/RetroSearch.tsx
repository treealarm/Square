import * as React from 'react';
import dayjs, { Dayjs } from 'dayjs';
import TextField from '@mui/material/TextField';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { ApiDefaultPagingNum, ApplicationState } from '../store';
import { Box, ButtonGroup, FormControlLabel, IconButton, List, ListItem, Switch } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import * as SearchResultStore from '../store/SearchResultStates';
import { ObjPropsSearchDTO, SearchFilter, SearchFilterDTO } from '../store/Marker';
import { PropertyFilter } from './PropertyFilter';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function RetroSearch() {
  const INPUT_FORMAT = "YYYY-MM-DDTHH:mm:ss";

  const dispatch = useDispatch();

  const [checkedTimeBegin, setCheckedTimeBegin] = React.useState(true);
  const [checkedTimeEnd, setCheckedTimeEnd] = React.useState(true);

  const handleCheckTimeBegin = (event: React.ChangeEvent<HTMLInputElement>) => {
    setCheckedTimeBegin(event.target.checked);
  };

  const handleCheckTimeEnd = (event: React.ChangeEvent<HTMLInputElement>) => {
    setCheckedTimeEnd(event.target.checked);
  };

  const searchFilter = useSelector((state) => state?.tracksStates?.searchFilter);

  function GetCopyOfSearchFilter(): SearchFilter {
    let filter = Object.assign({}, searchFilter);
    return filter;
  }

  useEffect(() => {
    if (searchFilter == null) {
      var filter: SearchFilter =
      {
        time_start: new Date(dayjs().subtract(1,"day").toISOString()),
        time_end: new Date(dayjs().toISOString()),
        property_filter: {
          props: [{ prop_name: "track_name", str_val: "lisa_alert" }]
        },
        search_id: "0"
      };
      dispatch(TracksStore.actionCreators.applyFilter(filter));
    }
  }, []);

  function UpdateFilterInRedux(filter: SearchFilter) {
    dispatch(TracksStore.actionCreators.applyFilter(filter));    
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

  const searchTracks = useCallback(
    (e) => {
      var filter: SearchFilter = GetCopyOfSearchFilter();
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

      if (!checkedTimeBegin) {
        filterDto.time_start = null;
      }
      if (!checkedTimeEnd) {
        filterDto.time_end = null;
      }
      dispatch(SearchResultStore.actionCreators.getByFilter(filterDto));

    }, [searchFilter]);

  const addProperty = useCallback(
    (e) => {
      var filter: SearchFilter = GetCopyOfSearchFilter();
      filter.property_filter.props.push({ prop_name: "test_name", str_val: "test_val" });
      UpdateFilterInRedux(filter);
    }, [searchFilter]);

  const setPropsFilter = useCallback(
    (propsFilter: ObjPropsSearchDTO) => {
      var filter: SearchFilter = GetCopyOfSearchFilter();
      filter.property_filter = propsFilter;
      UpdateFilterInRedux(filter);
    }, [searchFilter]);


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
            </ButtonGroup>
          </ListItem>
          <ListItem>
          <DateTimePicker
            label="begin (ISO 8601)"
            value={searchFilter?.time_start}
            onChange={handleChange1}
            inputFormat={INPUT_FORMAT}
            disabled={!checkedTimeBegin}
            renderInput={(params) =>
              <TextField {...params}
                inputProps={
                  {
                    ...params.inputProps,
                    placeholder: INPUT_FORMAT
                  } 
                }/>}
          />
          <FormControlLabel sx={{ padding: 1 }} control={
            <Switch defaultChecked size="small"
              checked={checkedTimeBegin}
              onChange={handleCheckTimeBegin} />
          } label="" />

          </ListItem>
          <ListItem>
          <DateTimePicker
            label="end (ISO 8601)"
            value={searchFilter?.time_end}
            onChange={handleChange2}
            inputFormat={INPUT_FORMAT}
            disabled={!checkedTimeEnd}
            renderInput={(params) =>
              <TextField {...params}
                inputProps={
                  {
                    ...params.inputProps,
                    placeholder: INPUT_FORMAT
                  }
                } />}
          />

          <FormControlLabel sx={{ padding: 1 }} control={
            <Switch defaultChecked size="small"
              checked={checkedTimeEnd}
              onChange={handleCheckTimeEnd} />
          } label="" />
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

