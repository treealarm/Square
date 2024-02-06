import * as React from "react";

import {
  Box, Grid, Toolbar
} from "@mui/material";
import DiagramTypeViewer from "../diagramtypeeditor/DiagramTypeViewer";
import { DiagramTypeProperties } from "../diagramtypeeditor/DiagramTypeProperties";
import { DiagramTypeSearcher } from "../diagramtypeeditor/DiagramTypeSearcher";

export function DiagramTypeEditor() {

  return (
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >
        <Toolbar />
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