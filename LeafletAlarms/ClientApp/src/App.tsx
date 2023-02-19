import * as React from "react";
import { Route } from "react-router";
import { Layout } from "./components/Layout";
import { Home } from "./components/Home";
import "./custom.css";
import { Box, createTheme, ThemeProvider } from "@mui/material";
import { useAppDispatch } from ".";
import * as RightsStore from './store/RightsStates';
import UserService from "./auth/UserService";
import { useSelector } from "react-redux";
import { ApplicationState } from "./store";

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
  }, []);


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
          <Route exact path="/" component={Home} />
        </Layout>
      </Box>
    </ThemeProvider>
  );

}
