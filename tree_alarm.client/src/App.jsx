/* eslint-disable react-hooks/exhaustive-deps */
import "./custom.css";

import { BrowserRouter as Router, Routes, Route } from "react-router-dom";

import { Layout } from "./components/Layout";
import { Home } from "./components/Home";
import { DiagramTypeEditor } from "./diagramtypeeditor/DiagramTypeEditor";
import { DiagramEditWorkspace } from "./editworkspace/DiagramEditWorkspace";
import { EventViewer } from "./eventviewer/EventViewer";
import { StatesViewer } from "./statesviewer/statesviewer";
import { MonitorViewer } from "./monitorviewer/MonitorViewer";
import { ObjectPropertiesUpdater } from "./components/ObjectPropertiesUpdater";

import { AuthGuard } from "./auth/AuthGuard";
import { LoginForm } from "./auth/LoginForm";

import { createTheme, ThemeProvider } from "@mui/material";

const theme = createTheme({
  spacing: 3,
  typography: { button: { textTransform: "none" } },
  palette: {
    mode: "light",
    primary: { main: "#3f51b5" },
    secondary: { main: "#f50057" },
  },
});

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <Router>
        <ObjectPropertiesUpdater />

        {/* ���������� ������ */}
        <AuthGuard />

        <Routes>
          {/* ������ �������� */}
          <Route path="/login" element={<LoginForm />} />

          {/* ���������� �������� */}
          <Route path="/" element={<Layout><Home /></Layout>} />
          <Route path="/_editdiagrams" element={<Layout><DiagramEditWorkspace /></Layout>} />
          <Route path="/editdiagram" element={<Layout><DiagramTypeEditor /></Layout>} />
          <Route path="/_events" element={<Layout><EventViewer /></Layout>} />
          <Route path="/_states" element={<Layout><StatesViewer /></Layout>} />
          <Route path="/_monitor" element={<Layout><MonitorViewer /></Layout>} />
        </Routes>
      </Router>
    </ThemeProvider>
  );
}
