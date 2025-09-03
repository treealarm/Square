/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
import { useSelector } from "react-redux";
import { useCallback, useEffect, useState } from "react";
import {
  Box, Button, Grid, Toolbar
} from "@mui/material";

import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";

import * as GuiStore from '../store/GUIStates';

import StatesTable from './statestable';       // таблица состо€ний
import { StateProperties } from './stateproperties'; // отдельный компонент дл€ детальной инфы

import * as MarkersVisualStore from '../store/MarkersVisualStates';
import { IObjectStateDTO } from "../store/Marker";
import { useNavigate } from "react-router-dom";
import { TreeControl } from "../tree/TreeControl";

export function StatesViewer() {
  const appDispatch = useAppDispatch();

  const navigate = useNavigate();

  const selectedState: IObjectStateDTO | null = useSelector(
    (state: ApplicationState) => state?.markersVisualStates?.selected_state_id ?? null
  );

  useEffect(() => {
    appDispatch(GuiStore.set_cur_interface("_states"));
  }, []);

  // обработчик выбора строки
  const handleSelect = useCallback((row: IObjectStateDTO | null) => {
    if (selectedState?.id === row?.id) {
      appDispatch(MarkersVisualStore.set_selected_state(null));
    } else {
      appDispatch(MarkersVisualStore.set_selected_state(row));
    }
  }, [selectedState]);

  const goHome = () => {
    navigate("/"); // переход на главную
  };

  return (
    <Box sx={{ height: "98vh", display: "flex", flexDirection: "column" }}>

      <Toolbar sx={{ backgroundColor: "lightgray", justifyContent: "space-between" }}>
        
        <Button variant="contained" onClick={goHome}>
          Home
        </Button>

        <Box>States Viewer</Box>
      </Toolbar>

      <Grid
        container
        sx={{
          height: "100%",
          width: "100%",
          overflow: "auto",
          flex: 1
        }}
      >
        <Grid item xs={3} sx={{ height: "100%"}}>
          <TreeControl key={"TreeControl 1"} />
        </Grid>
        {/* лева€ часть Ч таблица состо€ний */}
        <Grid item xs sx={{ minWidth: "100px", minHeight: "100px", height: "100%", border: 1 }}>
          <StatesTable onSelect={handleSelect} />
        </Grid>

        {/* права€ часть Ч панель свойств */}
        <Grid item xs={3} sx={{ height: "100%", border: 1 }}>
          <StateProperties />
        </Grid>
      </Grid>
    </Box>
  );
}

