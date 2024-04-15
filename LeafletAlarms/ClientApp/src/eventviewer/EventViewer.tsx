import * as React from "react";
import { useNavigate } from "react-router-dom";
import { useSelector } from "react-redux";

import {
  Box, ToggleButton, ButtonGroup, Grid, IconButton, Toolbar, Tooltip
} from "@mui/material";
import Stack from '@mui/material/Stack';
import Divider from '@mui/material/Divider';
import TextField from '@mui/material/TextField';
import InputAdornment from '@mui/material/InputAdornment';
import FirstPageIcon from '@mui/icons-material/FirstPage';
import SearchIcon from '@mui/icons-material/Search';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';

import * as EventsStore from '../store/EventsStates'

import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { EventProperties } from "./EventProperties";
import EventTable from "./EventTable";
import { DeepCopy, SearchFilterDTO } from "../store/Marker";
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import dayjs, { Dayjs } from 'dayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { INPUT_DATETIME_FORMAT } from "../store/constants";
import { useEffect, useState } from "react";

function uuidv4() {
  return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
    (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
  );
}
export function EventViewer() {

  const appDispatch = useAppDispatch();
  //const guid = useId();
  //console.log("guid=", guid);
  const [autoUpdate, setAutoUpdate] = useState(false);
  const searchFilter: SearchFilterDTO = useSelector((state: ApplicationState) => state?.eventsStates?.filter);
  const isFetching: boolean = useSelector((state: ApplicationState) => state?.eventsStates?.isFetching);

  let timeoutId: any = null;

  useEffect(() => {
    if (timeoutId != null) {
      clearTimeout(timeoutId);
    }

    if (searchFilter.search_id == null) {
      // Make cursor on start.
      var newFilter = DeepCopy(searchFilter);
      setLocalFilter(newFilter);
      return;
    }
    timeoutId = setTimeout(() => {
      appDispatch(EventsStore.fetchEventsByFilter(searchFilter));
    }, 1000);
    return () => clearTimeout(timeoutId);
  }, [searchFilter]);

  useEffect(() => {
    // Just reserve cursor if exist.

    if (!autoUpdate || isFetching) {
      return;
    }
    const interval = setInterval(() => {
      appDispatch(EventsStore.fetchEventsByFilter(searchFilter));
    }
      , 5000);
    return () => {
      clearInterval(interval);
    };
  }, [searchFilter, autoUpdate, isFetching]);

  useEffect(() => {
    // Autoupdate
    const interval = setInterval(() => {
      if (searchFilter?.forward != 0) {
        appDispatch(EventsStore.reserveCursor(searchFilter?.search_id));
      }
    }
      , 30000);
    return () => {
      clearInterval(interval);
    };
  }, [searchFilter]);

  function setLocalFilter(newFilter: SearchFilterDTO) {

    var idFromStorage = sessionStorage.getItem("ID_KEY");
    if (idFromStorage == null) {
      idFromStorage = uuidv4();
      sessionStorage.setItem("ID_KEY", idFromStorage);
    }
    newFilter.search_id = idFromStorage;

    appDispatch(EventsStore.set_local_filter(newFilter));
  }

  const handleChange1 = (newValue: Dayjs | null) => {
    try {
      var newFilter = DeepCopy(searchFilter);
      newFilter.time_start = newValue.toISOString();
      newFilter.forward = 0;// replace cursor
      setLocalFilter(newFilter);
    }
    catch (err) {

    }
  };

  const handleChange2 = (newValue: Dayjs | null) => {
    try {
      var newFilter = DeepCopy(searchFilter);
      newFilter.time_end = newValue.toISOString();
      newFilter.forward = 0;// replace cursor
      setLocalFilter(newFilter);
    }
    catch (err) {

    }
  };
  function handleChangeTextSearch(e: any) {
    const { target: { id, value } } = e;
    var newFilter = DeepCopy(searchFilter);
    newFilter.forward = 0;// replace cursor
    newFilter.start_id = value;
    setLocalFilter(newFilter);
  };

  const OnNavigate = (next: number) => {

    if (next == 0) {
      setAutoUpdate(!autoUpdate);
    }
    var newFilter = DeepCopy(searchFilter);
    newFilter.forward = next;
    setLocalFilter(newFilter);
  }

  return (
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >

        <LocalizationProvider dateAdapter={AdapterDayjs}>
          <Toolbar sx={{ backgroundColor: 'lightgray', justifyContent: "center", }}>
            <Box>
              <Stack
                direction="row"
                divider={<Divider orientation="vertical" flexItem />}
                spacing={2}
              >
                <ButtonGroup>
                  <Tooltip title={"First events page " + (!autoUpdate ? "/ autoupdate on" : "/ autoupdate off")}>
                    <ToggleButton
                      sx={{ borderRadius: 1, border: 0 }}
                      value="check"
                      selected={autoUpdate}
                      onClick={(e: any) => OnNavigate(0)}>
                      <FirstPageIcon />
                    </ToggleButton >
                  </Tooltip>
                  <Divider orientation="vertical" variant="middle" flexItem><br /></Divider>
                  <Tooltip title={"Next events page"}>
                    <IconButton
                      sx={{ borderRadius: 1, border: 0 }}
                      onClick={(e: any) => OnNavigate(1)}>
                      <NavigateNextIcon />
                    </IconButton>
                  </Tooltip>
                </ButtonGroup>



                <TextField
                  id="input-with-icon-textfield"
                  size="small"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon />
                      </InputAdornment>
                    ),
                  }}
                  variant="standard"
                  value={searchFilter?.start_id == null ? '' : searchFilter.start_id }
                  onChange={handleChangeTextSearch}
                />


                <DateTimePicker
                  label="begin"
                  value={dayjs(searchFilter?.time_start)}
                  onChange={handleChange1}
                  views={['year', 'month', 'day', 'hours', 'minutes', 'seconds']}
                  format={INPUT_DATETIME_FORMAT}
                  slotProps={{ textField: { size: 'small' } }}
                />

                <DateTimePicker
                  label="end"
                  value={dayjs(searchFilter?.time_end)}
                  onChange={handleChange2}
                  format={INPUT_DATETIME_FORMAT}
                  slotProps={{ textField: { size: 'small' } }}
                />
              </Stack>
            </Box>
          </Toolbar>
        </LocalizationProvider>

      </Box>

      <Grid container sx={{
        height: '100%',
        width: '100%',
        overflow: 'auto',
        flex: 1
      }}>

        <Grid item xs sx={{ minWidth: '100px', minHeight: '100px', height: '100%', border: 1 }}>
          <EventTable setLocalFilter={setLocalFilter} />
        </Grid>

        <Grid item xs={3} sx={{ height: "100%", border: 1 }}>
          <EventProperties />
        </Grid>
      </Grid>

    </Box>
  );
}