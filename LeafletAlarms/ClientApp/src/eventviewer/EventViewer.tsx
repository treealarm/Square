import * as React from "react";
import { useNavigate } from "react-router-dom";
import { useSelector } from "react-redux";

import {
  Box, Button, ButtonGroup, Grid, IconButton, Toolbar, Tooltip
} from "@mui/material";

import FirstPageIcon from '@mui/icons-material/FirstPage';
import NavigateBeforeIcon from '@mui/icons-material/NavigateBefore';
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
import { useEffect, useId } from "react";

function uuidv4() {
  return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
    (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
  );
}
export function EventViewer() {

  const appDispatch = useAppDispatch();
  const guid = useId();
  const searchFilter: SearchFilterDTO = useSelector((state: ApplicationState) => state?.eventsStates?.filter);

  let timeoutId: any = null;
  
  useEffect(() => {
    if (timeoutId != null) {
      clearTimeout(timeoutId);
    }

    timeoutId = setTimeout(() => {
      appDispatch(EventsStore.fetchEventsByFilter(searchFilter));
    }, 1000);
    return () => clearTimeout(timeoutId);
  }, [searchFilter]);

  useEffect(() => {
    // Just reserve cursor if exist.
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
      setLocalFilter(newFilter);
    }
    catch (err) {

    }
  };

  const handleChange2 = (newValue: Dayjs | null) => {
    try {
      var newFilter = DeepCopy(searchFilter);
      newFilter.time_end = newValue.toISOString();
      setLocalFilter(newFilter);
    }
    catch (err) {

    }
  };
  const OnNavigate = (next: number) => {

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
          <Toolbar sx={{ justifyContent: "left", backgroundColor: 'lightgray' }}>
            <Box>
              <ButtonGroup>
                <Tooltip title={"First events page"}>
                  <IconButton onClick={(e: any) => OnNavigate(0)}>
                    <FirstPageIcon />
                  </IconButton>
                </Tooltip>

                <Tooltip title={"Next events page"}>
                  <IconButton onClick={(e: any) => OnNavigate(1)}>
                    <NavigateNextIcon />
                  </IconButton>
                </Tooltip>
              </ButtonGroup>

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