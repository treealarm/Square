import * as React from "react";
import { useNavigate } from "react-router-dom";
import { useSelector } from "react-redux";

import {
  Box, Button, ButtonGroup, Grid, Toolbar
} from "@mui/material";
import * as DiagramTypeStore from '../store/DiagramTypeStates'

import DiagramTypeViewer from "../diagramtypeeditor/DiagramTypeViewer";
import { DiagramTypeProperties } from "../diagramtypeeditor/DiagramTypeProperties";
import { DiagramTypeSearcher } from "../diagramtypeeditor/DiagramTypeSearcher";

import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";

export function DiagramTypeEditor() {

  const appDispatch = useAppDispatch();
  const result = useSelector((state: ApplicationState) => state?.diagramtypeStates?.result);
  let navigate = useNavigate();

  function onClickOk() {
    appDispatch(DiagramTypeStore.set_result('OK')); 
    navigate(-1);
  }
  function onClickCancel() {
    appDispatch(DiagramTypeStore.set_result('CANCEL')); 
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
        
        <Grid item xs={3} sx={{ height: "100%", border:1 }}>
          <DiagramTypeSearcher />
        </Grid>
        <Grid item xs sx={{ minWidth: '100px', minHeight: '100px', height: '100%', border: 1 }}>
          <DiagramTypeViewer/>
        </Grid>

        <Grid item xs={3} sx={{ height: "100%", border: 1 }}>
          <DiagramTypeProperties />
        </Grid>
      </Grid>

    </Box>
  );
}