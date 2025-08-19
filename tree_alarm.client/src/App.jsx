/* eslint-disable no-unused-vars */
import * as React from "react";
import "./custom.css";

import { BrowserRouter as Router, Routes ,Route  } from 'react-router-dom';


import { Layout } from "./components/Layout.tsx";
import { Home } from "./components/Home.tsx";
import { DiagramTypeEditor } from "./diagramtypeeditor/DiagramTypeEditor.tsx";
import { EventViewer } from "./eventviewer/EventViewer.tsx";
import { StatesViewer } from "./statesviewer/statesviewer.tsx";

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
    mode: 'light',
    primary: {
      main: '#3f51b5',
    },
    secondary: {
      main: '#f50057',
    },
  },

});

export default function App() {
  const appDispatch = useAppDispatch();


  React.useEffect(() => {
    function setToken() {
      appDispatch(RightsStore.set_user(UserService.getUsername()));
    }

    function onUserChangedCallback() {
      appDispatch(RightsStore.set_user(UserService.getUsername()));
    }
    if (!UserService.isLoggedIn()) {
      UserService.initKeycloak(setToken, onUserChangedCallback);
    }
  });


  return (
    <ThemeProvider theme={theme}>
      <Router>
        <Routes>
          <Route path="/" element={<Layout><Home /></Layout>} />
          <Route path="/editdiagram" element={<Layout><DiagramTypeEditor /></Layout>} />
          <Route path="/_events" element={<Layout><EventViewer /></Layout>} />
          <Route path="/_states" element={<Layout><StatesViewer/></Layout>} />
        </Routes>
      </Router>
    </ThemeProvider>
  );


}
