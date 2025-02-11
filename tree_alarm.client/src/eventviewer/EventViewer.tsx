/* eslint-disable no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
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
import ClearOutlinedIcon from '@mui/icons-material/ClearOutlined';

import * as EventsStore from '../store/EventsStates'

import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { EventProperties } from "./EventProperties";
import EventTable from "./EventTable";
import { DeepCopy, ObjPropsSearchDTO, SearchEventFilterDTO, uuidv4 } from "../store/Marker";
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import dayjs, { Dayjs } from 'dayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { INPUT_DATETIME_FORMAT } from "../store/constants";
import { useCallback, useEffect, useRef, useState } from "react";
import { PropertyFilterEditor } from "./PropertyFilterEditor";

const TimePicker = ({ label, value, onChange }: { label: string, value: string | null, onChange: (newValue: Dayjs | null) => void }) => (
  <DateTimePicker
    label={label}
    value={dayjs(value)}
    onChange={onChange}
    format={INPUT_DATETIME_FORMAT}
    slotProps={{
      textField: { size: 'small' },
      actionBar: {
        actions: ['accept', 'cancel', 'clear', 'today']
      }
    }}
  />
);


export function EventViewer() {

  const appDispatch = useAppDispatch();
  //const guid = useId();
  //console.log("guid=", guid);
  const [autoUpdate, setAutoUpdate] = useState(false);
  const searchFilter: SearchEventFilterDTO |null = useSelector((state: ApplicationState) => state?.eventsStates?.filter)??null;
  const isFetching: boolean = useSelector((state: ApplicationState) => state?.eventsStates?.isFetching);

  const timeoutIdRef = useRef<any>(null);

  const setLocalFilter = useCallback((newFilter: SearchEventFilterDTO) => {

    var idFromStorage = sessionStorage.getItem("ID_KEY");
    if (idFromStorage == null) {
      idFromStorage = uuidv4();
      sessionStorage.setItem("ID_KEY", idFromStorage);
    }
    newFilter.search_id = idFromStorage;

    appDispatch(EventsStore.set_local_filter(newFilter));
  }, [])

  useEffect(() => {
    if (timeoutIdRef.current != null) {
      clearTimeout(timeoutIdRef.current);
    }

    if (searchFilter.search_id == null) {
      // Make cursor on start.
      var newFilter = DeepCopy(searchFilter);
      setLocalFilter(newFilter);
      return;
    }
    timeoutIdRef.current = setTimeout(() => {
      appDispatch(EventsStore.fetchEventsByFilter(searchFilter));
    }, 1000);

    return () => clearTimeout(timeoutIdRef.current);
  }, [searchFilter, setLocalFilter]);

  useEffect(() => {
    // Just reserve cursor if exist.

    if (!autoUpdate || isFetching) {
      return;
    }
    const interval = setInterval(() => {
      appDispatch(EventsStore.fetchEventsByFilter(searchFilter));
    }
      , 3000);
    return () => {
      clearInterval(interval);
    };
  }, [searchFilter, autoUpdate, isFetching, appDispatch]);

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
  }, [appDispatch, searchFilter]);

  const handleChange1 = (newValue: Dayjs | null) => {
    try {
      var newFilter = DeepCopy(searchFilter);
      if (newFilter) {
        newFilter.time_start = newValue ? newValue.toISOString() : null;
        newFilter.forward = 0;// replace cursor
        setLocalFilter(newFilter);
      }      
    }
    catch (err) {
      console.log(err);
    }
  };

  const handleChange2 = (newValue: Dayjs | null) => {
    try {
      var newFilter = DeepCopy(searchFilter);
      newFilter.time_end = newValue ? newValue.toISOString() : null;
      newFilter.forward = 0;// replace cursor
      setLocalFilter(newFilter);
    }
    catch (err) {
      console.log(err);
    }
  };

  function clearTextSearch() {
    var newFilter = DeepCopy(searchFilter);
    newFilter.forward = 0;// replace cursor
    newFilter.start_id = "";
    setLocalFilter(newFilter);
  }

  function handleChangeTextSearch(e: any) {
    const { target: { value } } = e;
    var newFilter = DeepCopy(searchFilter);
    newFilter.forward = 0;// replace cursor
    newFilter.start_id = value;
    setLocalFilter(newFilter);
  }

  function OnNavigate (next: number){

    if (next == 0) {
      setAutoUpdate(!autoUpdate);
    }
    var newFilter = DeepCopy(searchFilter);
    newFilter.forward = next;
    setLocalFilter(newFilter);
  }

  const handleFilterChange = useCallback((updatedFilterData: ObjPropsSearchDTO) => {
    var newFilter = DeepCopy(searchFilter);
    newFilter.forward = 0; // replace cursor
    newFilter.property_filter = updatedFilterData;
    setLocalFilter(newFilter);
  }, [searchFilter]);


  return (
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >

        <LocalizationProvider dateAdapter={AdapterDayjs}>
          <Toolbar sx={{ backgroundColor: 'lightgray', justifyContent: "center", }}>
              <Stack
                direction="row"                
                divider={<Divider orientation="vertical" flexItem />}
                spacing={1}
              >
                <ButtonGroup>
                  <Tooltip title={"First events page " + (!autoUpdate ? "/ autoupdate on" : "/ autoupdate off")}>
                    <ToggleButton
                      sx={{ borderRadius: 1, border: 0 }}
                      value="check"
                      selected={autoUpdate}
                      onClick={() => OnNavigate(0)}>
                      <FirstPageIcon />
                    </ToggleButton >
                  </Tooltip>
                  <Divider orientation="vertical" variant="middle" flexItem><br /></Divider>
                  <Tooltip title={"Next events page"}>
                    <IconButton
                      sx={{ borderRadius: 1, border: 0 }}
                      onClick={() => OnNavigate(1)}>
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
                    endAdornment: (
                      <IconButton onClick={() => clearTextSearch()}>
                        {searchFilter?.start_id?.length > 0 ? <ClearOutlinedIcon /> : ''}
                      </IconButton>
                    )
                  }}
                  variant="standard"
                  value={searchFilter?.start_id == null ? '' : searchFilter.start_id }
                  onChange={handleChangeTextSearch}
                />

              <PropertyFilterEditor onChange={handleFilterChange} />
              <TimePicker label="begin" value={searchFilter?.time_start} onChange={handleChange1} />
              <TimePicker label="end" value={searchFilter?.time_end} onChange={handleChange2} />

              </Stack>

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