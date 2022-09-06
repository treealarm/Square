import * as React from 'react';
import dayjs, { Dayjs } from 'dayjs';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { ApplicationState } from '../store';
import { Box, ButtonGroup, IconButton, List, ListItem } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import { ObjPropsSearchDTO, SearchFilter } from '../store/Marker';
import { PropertyFilter } from './PropertyFilter';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function RetroSearch() {
  const INPUT_FORMAT = "YYYY-MM-DDTHH:mm:ss";

  const dispatch = useDispatch();

  const [value1, setValue1] = React.useState<Dayjs | null>(
    //dayjs('2014-08-18T21:11:54'),
    dayjs()
  );
  const [value2, setValue2] = React.useState<Dayjs | null>(
    //dayjs('2014-08-18T21:11:54'),
    dayjs()
  );

  const [propsFilter, setPropsFilter] = React.useState<ObjPropsSearchDTO>({
    props: [{ prop_name: "track_name", str_val: "lisa_alert"}]
  });

  const handleChange1 = (newValue: Dayjs | null) => {
    setValue1(newValue);
  };

  const handleChange2 = (newValue: Dayjs | null) => {
    setValue2(newValue);
  };

  const searchTracks = useCallback(
    (e) => {

      var filter: SearchFilter =
      {
        time_start:new Date(value1.toISOString()),
        time_end: new Date(value2.toISOString()),
        property_filter: propsFilter
      };
      dispatch(TracksStore.actionCreators.applyFilter(filter));
    }, [value1, value2, propsFilter]);

  const addProperty = useCallback(
    (e) => {
      let copy = Object.assign({}, propsFilter);
      copy.props.push({ prop_name: "test_name", str_val: "test_val" });
      setPropsFilter(copy);
    }, [propsFilter]);

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
            value={value1}
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
          </ListItem>
          <ListItem>
          <DateTimePicker
            label="end (ISO 8601)"
            value={value2}
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
          </ListItem>
          <ListItem>
          <PropertyFilter propsFilter={propsFilter} setPropsFilter={setPropsFilter} />
          </ListItem>
        </List>
      </LocalizationProvider>
  );
}

