import * as React from "react";
import { useNavigate } from "react-router-dom";
import { useSelector } from "react-redux";

import {
  Box, Button, ButtonGroup, Grid, Toolbar
} from "@mui/material";
import * as EventsStore from '../store/EventsStates'

import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { EventProperties } from "./EventProperties";
import EventTable from "./EventTable";
import { DeepCopy, SearchFilterDTO } from "../store/Marker";

export function EventViewer() {

  const appDispatch = useAppDispatch();
  const result = useSelector((state: ApplicationState) => state?.diagramtypeStates?.result);
  let navigate = useNavigate();

  const localFilter = useSelector((state: ApplicationState) => state?.eventsStates?.filter);

  let timeoutId: any = null;

  React.useEffect(() => {
    if (timeoutId != null) {
      clearTimeout(timeoutId);
    }

    timeoutId = setTimeout(() => {
      appDispatch(EventsStore.fetchEventsByFilter(localFilter));
    }, 1000);
    return () => clearTimeout(timeoutId);
  }, [localFilter]);

  function onClickOk() {

    navigate(-1);
  }
  function onClickCancel() {

    navigate(-1);
  }
  return (
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >
        <Toolbar sx={{ justifyContent: "left", backgroundColor: 'lightgray' }}>
          {
            result == null ? null :
              <Box>
                <ButtonGroup>
                  <Button
                    key={'button ok'}
                    onClick={onClickOk}
                  >
                    Ok
                  </Button>
                  <Button
                    key={'button cancel'}
                    onClick={onClickCancel}
                  >
                    Cancel
                  </Button>
                </ButtonGroup>
              </Box>
     
          }
        </Toolbar>
      </Box>  

      <Grid container sx={{
        height: '100%',
        width: '100%',
        overflow: 'auto',
        flex: 1
      }}>
        
        <Grid item xs sx={{ minWidth: '100px', minHeight: '100px', height: '100%', border: 1 }}>
          <EventTable />
        </Grid>

        <Grid item xs={3} sx={{ height: "100%", border: 1 }}>
          <EventProperties />
        </Grid>
      </Grid>

    </Box>
  );
}