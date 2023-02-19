import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import {
  Accordion, AccordionDetails, AccordionSummary,
  Checkbox,
  FormControlLabel,
  FormGroup,
  Grid, Paper, Stack, Typography
} from "@mui/material";
import { WebSockClient } from "./WebSockClient";
import { RetroSearch } from "../Tree/RetroSearch";
import { SearchResult } from "../Tree/SearchResult";
import GlobalLayersOptions from "../Tree/GlobalLayersOptions";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { ObjectLogic } from "../Logic/ObjectLogic";
import { Login } from "../auth/Login";
import { ObjectRights } from "../Rights/ObjectRights";


export function Home() {

  const [showPannel, setShowPannel] = React.useState(true);

  const handleShowPannel = (event: React.ChangeEvent<HTMLInputElement>) => {
    setShowPannel(event.target.checked);
  };


  return (
    <Grid container spacing={1} sx={{ height: "100%", p: "1px" }}>
      <Grid item xs={12} sx={{ height: "auto" }}>
        <Stack direction="row" spacing={1}>

          <FormGroup row>

            <FormControlLabel control={
              <Checkbox
                checked={showPannel}
                id="CheckBoxShowPannel"
                onChange={handleShowPannel}
                size="small"
                tabIndex={-1}
                disableRipple
              />}
                  label="Show panels"
                />
              
          </FormGroup>

          <WebSockClient />
          <GlobalLayersOptions />
          <Login/>
        </Stack>
      </Grid>

      <Grid item xs={2} sx={{ height: "90%", display: showPannel ? '' : 'none' }}
        container spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width: "100%" }} >
          <Accordion>
            <AccordionSummary              
              expandIcon={<ExpandMoreIcon color="primary"/>}
              aria-controls="panel-tree"
              id="panel-tree">
              <Typography color="primary">Tree</Typography>
            </AccordionSummary>
            <AccordionDetails sx={{ maxHeight: "100%", padding: 1, margin: 0 }} >
              <TabControl />
              <TreeControl />
            </AccordionDetails>
          </Accordion>
          <Accordion>
            <AccordionSummary
              expandIcon={<ExpandMoreIcon color="secondary" />}
              aria-controls="panel-search-result"
              id="panel-search-result">
              <Typography color="secondary">Search result</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <SearchResult></SearchResult>
            </AccordionDetails>
          </Accordion>
        </Paper>

      </Grid>

      <Grid item xs={showPannel ? 7 : 12} sx={{ height: "90%" }} container spacing={0}>
        <MapComponent/>
      </Grid>

      <Grid item xs={3} sx={{ height: "90%", display: showPannel ? '' : 'none' }}
        container spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width: "100%" }}>
          <Accordion>
            <AccordionSummary              
              expandIcon={<ExpandMoreIcon color="primary" />}
              aria-controls="panel1a-content"              
              id="panel1a-header">
              <Typography color="primary">Properties</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <Stack sx={{ height: "100%" }}>
                <EditOptions />
                <ObjectProperties />
              </Stack>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary
              expandIcon={<ExpandMoreIcon color="secondary" />}
              aria-controls="panel-properties"
              id="panel-properties"
            >
              <Typography color="secondary">Search</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <RetroSearch></RetroSearch>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary
              expandIcon={<ExpandMoreIcon color="action" />}
              aria-controls="panel-logic"
              id="panel-logic"
            >
              <Typography color="action">Logic</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <ObjectLogic/>
            </AccordionDetails>
          </Accordion>

          <Accordion>
            <AccordionSummary
              expandIcon={<ExpandMoreIcon color="action" />}
              aria-controls="panel-logic"
              id="panel-logic"
            >
              <Typography color="warning">Rights</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <ObjectRights />
            </AccordionDetails>
          </Accordion>

        </Paper>

      </Grid>
    </Grid>
  );
}
