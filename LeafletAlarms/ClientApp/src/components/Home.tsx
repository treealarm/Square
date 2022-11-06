import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import { Box, Grid, Paper, Stack, Tab } from "@mui/material";
import { WebSockClient } from "./WebSockClient";
import { TabContext, TabList, TabPanel } from "@mui/lab";
import { RetroSearch } from "../Tree/RetroSearch";
import { SearchResult } from "../Tree/SearchResult";

export function Home() {

  const [valuePropTab, setValuePropTab] = React.useState('1');
  const handleChangePropTab = (event: React.SyntheticEvent, newValue: string) => {
    setValuePropTab(newValue);
  };

  const [LeftTabValue, setLeftTabValue] = React.useState('1');
  const handleChangeTabValue = (event: React.SyntheticEvent, newValue: string) => {
    setLeftTabValue(newValue);
  };

  return (
    <Grid container spacing={1} sx={{ height: "100%", p: "1px" }}>
      <Grid item xs={12} sx={{ height: "auto" }}>
        <Stack direction="row" spacing={1}>
          <TabControl />
          <WebSockClient />
          <EditOptions />
        </Stack>
      </Grid>

      <Grid item xs={2} sx={{ height: "90%" }} container spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto' }} >
          <TabContext value={LeftTabValue} >
            <TabList
              variant="scrollable"
              scrollButtons
              allowScrollButtonsMobile
              onChange={handleChangeTabValue} aria-label="Tree tabs">
              <Tab label="Tree" value="1" />
              <Tab label="Search Result" value="2" />
            </TabList>
            <TabPanel value="1" sx={{ padding:1}}>
              <TreeControl />
            </TabPanel>
            <TabPanel value="2" sx={{ padding: 1 }}>
              <SearchResult></SearchResult>
            </TabPanel>
          </TabContext>
        </Paper>
      </Grid>

      <Grid item xs={7} sx={{ height: "90%" }} container spacing={0}>
        <MapComponent propTab = {valuePropTab}/>
      </Grid>

      <Grid item xs={3} sx={{ height: "90%" }} container spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width:"100%"}}>
          <TabContext value={valuePropTab}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <TabList
                variant="scrollable"
                scrollButtons
                allowScrollButtonsMobile
                onChange={handleChangePropTab} aria-label="Property tabs">
                <Tab label="Properties" value="1" />
                <Tab label="Search" value="2" />
              </TabList>
            </Box>
            <TabPanel value="1" sx={{ padding: 1 }}>
              <Stack sx={{ height: "100%" }}>                
                <ObjectProperties />
              </Stack>
            </TabPanel>
            <TabPanel value="2" sx={{ padding: 1 }}>
              <RetroSearch></RetroSearch>
            </TabPanel>
          </TabContext>
        </Paper>

      </Grid>

    </Grid>
  );
}
