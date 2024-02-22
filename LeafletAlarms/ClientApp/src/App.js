import * as React from "react";
import "./custom.css";

import { BrowserRouter as Router, Routes ,Route  } from 'react-router-dom';


import { Layout } from "./components/Layout.tsx";
import { Home } from "./components/Home.tsx";
import { DiagramTypeEditor } from "./components/DiagramTypeEditor.tsx";

import { createTheme, ThemeProvider } from "@mui/material";
import * as RightsStore from './store/RightsStates.ts';
import UserService from "./auth/UserService.ts";

import { useAppDispatch } from './store/configureStore.ts';

const theme = createTheme({
  spacing: 3,

  typography: {    
    button: {
      textTransform: 'none'
    }
  },
  palette: {
    primary: {
      main: '#fff'      
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
            '&:focus': {
              border: '1px solid #000',
            },
          },
        },
      },
    },

    MuiSelect: {
      styleOverrides: {
        root: {
          '&:focus': {
            border: '1px solid #000',
          },
        },
      },
    },

    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          "&:hover .MuiOutlinedInput-notchedOutline": {
            borderColor: "#aaa", // Бордер при наведении
          },
          "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
            borderColor: "#aaa", // Бордер при фокусе
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
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          "& .MuiOutlinedInput-root:hover .MuiOutlinedInput-notchedOutline": {
            borderColor: "#aaa"            
          },
          "& .MuiOutlinedInput-root.Mui-focused  .MuiOutlinedInput-notchedOutline":
          {
            borderColor: "#aaa",
          },
          "& .MuiInputLabel-root.Mui-focused": { color: 'black' }
        },
      },
    },
    MuiTabs: {
      styleOverrides: {
        indicator: {
          height: 4
        }
      }
    }
  },
});

export default () => {
  const appDispatch = useAppDispatch();

  function setToken() {
    appDispatch(RightsStore.set_user(UserService.getUsername()));
  }

  function onUserChangedCallback() {
    appDispatch(RightsStore.set_user(UserService.getUsername()));
  }
  

  React.useEffect(() => {
    if (!UserService.isLoggedIn()) {
      UserService.initKeycloak(setToken, onUserChangedCallback);
    }
  }, [setToken, onUserChangedCallback]);


  return (
    <ThemeProvider theme={theme}>
      <Router>
        <Routes>
          <Route path="/" exact element={<Layout> <Home /> </Layout>} />
          <Route path="/editdiagram" exact element={<Layout> <DiagramTypeEditor /> </Layout>} />
        </Routes>
      </Router>
       
    </ThemeProvider>
  );

}
