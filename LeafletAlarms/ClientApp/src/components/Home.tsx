import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import { Grid, Stack } from "@mui/material";
import { WebSockClient } from "./WebSockClient";

export function Home() {
  return (
    <Grid container spacing={1} sx={{ height: "100%" }}>
      <Grid item xs={12} sx={{ height: "8%" }}>
        <Stack direction="row" spacing={2}><TabControl /><WebSockClient /></Stack>
      </Grid>

      <Grid item xs={2} sx={{ height: "90%" }}>
        <Stack sx={{ height: "100%" }}>          
          <TreeControl />
        </Stack>
      </Grid>

      <Grid item xs={8} sx={{ height: "90%" }}>
        <MapComponent />
      </Grid>
      <Grid item xs={2} sx={{ height: "90%" }}>
        <EditOptions />
        <ObjectProperties />
      </Grid>
    </Grid>
  );
}
