import * as React from "react";
import { Route } from "react-router";
import Layout from "./components/Layout";
import { Home } from "./components/Home";
import "./custom.css";
import { Box, createTheme, ThemeProvider } from "@mui/material";

const theme = createTheme({
  typography: {    
    button: {
      textTransform: 'none'
    }    
  }
});

export default () => (
  <ThemeProvider theme={theme}>
  <Box
    sx={{
      height: "95vh",
      border: 1,
      borderColor: 'primary.main'
    }}
  >
    <Layout>
      <Route exact path="/" component={Home} />
    </Layout>
    </Box>
  </ThemeProvider>
);
