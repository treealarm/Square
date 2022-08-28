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
import { SearchFilter } from '../store/Marker';

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
        time_end: new Date(value2.toISOString())
      };
      dispatch(TracksStore.actionCreators.applyFilter(filter));
    }, [value1, value2]);

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

