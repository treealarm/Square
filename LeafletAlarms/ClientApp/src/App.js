import * as React from "react";
import "./custom.css";

import { Layout } from "./components/Layout";
import { Home } from "./components/Home";
import { Box, createTheme, ThemeProvider } from "@mui/material";
import * as RightsStore from './store/RightsStates';
import UserService from "./auth/UserService";

import { useAppDispatch } from './store/configureStore';

const theme = createTheme({
  spacing: 3,

  typography: {    
    button: {
      textTransform: 'none'
    }
  },
  palette: {
    primary: {
      main: '#ffffff'      
    }    
  },
  components: {
    MuiListItem: {
      styleOverrides: {
        root: {
          backgroundColor: 'white',
          '&.Mui-selected, &.Mui-selected:hover': {
            backgroundColor: 'lightgray',
          },
        },
      },
    },
    MuiMenuItem: {
      styleOverrides: {
        root: {
          backgroundColor: 'white',
          '&.Mui-selected, &.Mui-selected:hover': {
            backgroundColor: 'lightgray',
          },
        },
      },
    },
    MuiListItemButton: {
      styleOverrides: {
        root: {
          backgroundColor: 'white',
          '&.Mui-selected, &.Mui-selected:hover': {
            backgroundColor: 'lightgray',
          },
        },
      },
    }
  },
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
        <Layout>
          <Home/>
        </Layout>
    </ThemeProvider>
  );

}
