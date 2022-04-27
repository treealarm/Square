import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import { Container, Grid, Stack } from "@mui/material";

export function Home() {
  return (
    <Grid container spacing={1} sx={{ height: "100%" }}>
      <Grid item xs={12} sx={{ height: "6%" }}>
        <TabControl />
      </Grid>

      <Grid item xs={2} sx={{ height: "94%" }}>
        <Stack sx={{ height: "100%" }}>
          <EditOptions />
          <TreeControl />
        </Stack>
      </Grid>

      <Grid item xs={8} sx={{ height: "94%" }}>
        <MapComponent />
      </Grid>
      <Grid item xs={2} sx={{ height: "94%" }}>
        <ObjectProperties />
      </Grid>
    </Grid>
  );
}
