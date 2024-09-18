
import {
  Box,
  Grid, Toolbar,
} from "@mui/material";

import { IntegrationToolbar } from './IntegrationToolbar';
import { IntegrationViewer } from './IntegrationViewer';
import { IntegrationLeafsViewer } from './IntegrationLeafsViewer';
import { ObjectProperties } from '../tree/ObjectProperties';
import { ObjectPropertiesUpdater } from '../components/ObjectPropertiesUpdater';
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";

export function IntegrationMain() {

  const reduxSelectedId = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

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

        <Grid item xs={2} sx={{ overflow: 'auto', height: "100%", boxShadow: 1 }}>
          <IntegrationViewer/>
        </Grid>

        <Grid item xs={4} sx={{ overflow: 'auto', minWidth: '100px', minHeight: '100px', height: '100%', boxShadow: 1 }}>
          <IntegrationLeafsViewer />
        </Grid>

        <Grid item xs sx={{ overflow: 'auto', height: "100%", boxShadow: 1 }}>
          <ObjectPropertiesUpdater />
          {
            reduxSelectedId !== null && reduxSelectedId !== undefined &&
            <div>
              <ObjectProperties />
            </div>
          }
        </Grid>
          
          
      </Grid>

    </Box>
  );
}
