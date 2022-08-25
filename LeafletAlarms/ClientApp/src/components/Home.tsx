import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import { Box, Grid, Stack, Tab } from "@mui/material";
import { WebSockClient } from "./WebSockClient";
import { TabContext, TabList, TabPanel } from "@mui/lab";
import { RetroSearch } from "../Tree/RetroSearch";

export function Home() {

  const [value, setValue] = React.useState('1');
  const handleChange = (event: React.SyntheticEvent, newValue: string) => {
    setValue(newValue);
  };

  return (
    <Grid container spacing={1} sx={{ height: "100%", p: "2px" }}>
      <Grid item xs={12} sx={{ height: "8%" }}>
        <Stack direction="row" spacing={2}><TabControl /><WebSockClient /></Stack>
      </Grid>

      <Grid item xs={2} sx={{ height: "90%" }}>
        <Stack sx={{ height: "100%" }}>          
          <TreeControl />
        </Stack>
      </Grid>

      <Grid item xs={7} sx={{ height: "90%" }}>
        <MapComponent />
      </Grid>
      <Grid item xs={3} sx={{ height: "90%" }}>

        <TabContext value={value}>
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <TabList
              variant="scrollable"
              scrollButtons
              allowScrollButtonsMobile
              onChange={handleChange} aria-label="Property tabs">
              <Tab label="Properties" value="1"/>
              <Tab label="Retrospective" value="2"/>
            </TabList>
          </Box>
          <TabPanel value="1">
            <Stack sx={{ height: "100%" }}>
              <EditOptions />
              <ObjectProperties />
            </Stack>
          </TabPanel>
          <TabPanel value="2">
            <RetroSearch></RetroSearch>
          </TabPanel>
        </TabContext>

        
      </Grid>
    </Grid>
  );
}
