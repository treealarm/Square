import * as React from 'react';

import {
  Box,
  Grid, Toolbar,
} from "@mui/material";

import { IntegrationToolbar } from './IntegrationToolbar';
import { IntegrationViewer } from './IntegrationViewer';

export function IntegrationMain() {

  return (
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >
        <IntegrationToolbar />
        <Toolbar />
      </Box>

      <Grid container sx={{
        height: '100%',
        width: '100%',
        overflow: 'auto',
        flex: 1
      }}>

        <Grid item xs={3} sx={{ height: "100%"}}>
          <IntegrationViewer/>
        </Grid>

        <Grid item xs sx={{ minWidth: '100px', minHeight: '100px', height: '100%' }}>

        </Grid>

        <Grid item xs={3} sx={{ height: "100%"}}>

        </Grid>
      </Grid>

    </Box>
  );
}
