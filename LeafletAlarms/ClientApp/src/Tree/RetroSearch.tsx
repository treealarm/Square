import * as React from 'react';
import dayjs, { Dayjs } from 'dayjs';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { ApplicationState } from '../store';
import { Box, ButtonGroup, IconButton } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import { ObjPropsSearchDTO, SearchFilter } from '../store/Marker';

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

  const [propName, setPropName] = React.useState("track_name");
  const [propVal, setPropVal] = React.useState("lisa_alert");

  const handleChange1 = (newValue: Dayjs | null) => {
    setValue1(newValue);
  };

  const handleChange2 = (newValue: Dayjs | null) => {
    setValue2(newValue);
  };

  function handleChangePropName(e: any) {
    const { target: { id, value } } = e;
    setPropName(value);
  };
  function handleChangePropVal(e: any) {
    const { target: { id, value } } = e;
    setPropVal(value);
  };

  const searchTracks = useCallback(
    (e) => {
      var pfilter: ObjPropsSearchDTO =
      {
        props: [{ prop_name: propName, str_val: propVal }]
      }

      var filter: SearchFilter =
      {
        time_start:new Date(value1.toISOString()),
        time_end: new Date(value2.toISOString()),
        property_filter: pfilter
      };
      dispatch(TracksStore.actionCreators.applyFilter(filter));
    }, [value1, value2, propName, propVal]);

  return (
    <Box
      sx={{
      width: '100%',
      maxWidth: 460,
      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <LocalizationProvider dateAdapter={AdapterDayjs}>
        <Stack spacing={3}
          sx={{
            m: 1
          }}>
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
          <TextField size="small"
            fullWidth
            id="prop_name" label="prop_name"
            value={propName}
            onChange={handleChangePropName} />
          <TextField size="small"
            fullWidth
            id="prop_val" label="prop_val"
            value={propVal}
            onChange={handleChangePropVal} />

          <ButtonGroup variant="contained" aria-label="search button group">
            <IconButton aria-label="search" size="large" onClick={(e) => searchTracks(e)}>
              <SearchIcon fontSize="inherit" />
            </IconButton>
          </ButtonGroup>

        </Stack>
      </LocalizationProvider>
    </Box>
  );
}

