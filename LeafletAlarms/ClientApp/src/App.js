import * as React from "react";
import { Route } from 'react-router-dom';
import { Layout } from "./components/Layout";
import { Home } from "./components/Home";
import "./custom.css";
import { Box, createTheme, ThemeProvider } from "@mui/material";
import { useAppDispatch } from ".";
import * as RightsStore from './store/RightsStates';
import UserService from "./auth/UserService";

const theme = createTheme({
  spacing: 4,
  typography: {    
    button: {
      textTransform: 'none'
    }
  }
});

export default () => {
  const appDispatch = useAppDispatch();

  function setToken() {
    appDispatch(RightsStore.set_user(UserService.getUsername()));
  }

  React.useEffect(() => {
    if (!UserService.isLoggedIn()) {
      UserService.initKeycloak(setToken);
    }
  }, [setToken]);


  return (
    <ThemeProvider theme={theme}>
      <Box
        sx={{
          height: "95vh",
          border: 1,
          borderColor: 'primary.main'
        }}
      >
        <Layout>
          <Home/>
        </Layout>
      </Box>
    </ThemeProvider>
  );

}
