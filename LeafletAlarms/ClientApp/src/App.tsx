import * as React from "react";
import { Route } from "react-router";
import Layout from "./components/Layout";
import { Home } from "./components/Home";
import "./custom.css";
import { Box } from "@mui/material";
import { red } from "@mui/material/colors";

export default () => (
  <Box
    sx={{
      height: "95vh",
      border: 5,
      borderColor: red
    }}
  >
    <Layout>
      <Route exact path="/" component={Home} />
    </Layout>
  </Box>
);
