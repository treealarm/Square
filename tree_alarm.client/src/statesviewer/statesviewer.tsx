/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
import { useSelector } from "react-redux";
import {
  Box, Button, Grid, Toolbar
} from "@mui/material";

import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";

import StatesTable from './StatesTable';       // ������� ���������
import { StateProperties } from './StateProperties'; // ��������� ��������� ��� ��������� ����

import * as MarkersVisualStore from '../store/MarkersVisualStates';
import { useCallback } from "react";
import { ObjectStateDTO } from "../store/Marker";
import { useNavigate } from "react-router-dom";

export function StatesViewer() {
  const appDispatch = useAppDispatch();

  const navigate = useNavigate();



  const selectedState: ObjectStateDTO | null = useSelector(
    (state: ApplicationState) => state?.markersVisualStates?.selected_state ?? null
  );

  // ���������� ������ ������
  const handleSelect = useCallback((row: ObjectStateDTO | null) => {
    if (selectedState?.id === row?.id) {
      appDispatch(MarkersVisualStore.set_selected_state(null));
    } else {
      appDispatch(MarkersVisualStore.set_selected_state(row));
    }
  }, [selectedState, appDispatch]);

  const goHome = () => {
    navigate("/"); // ������� �� �������
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
        {/* ����� ����� � ������� ��������� */}
        <Grid item xs sx={{ minWidth: "100px", minHeight: "100px", height: "100%", border: 1 }}>
          <StatesTable onSelect={handleSelect} />
        </Grid>

        {/* ������ ����� � ������ ������� */}
        <Grid item xs={3} sx={{ height: "100%", border: 1 }}>
          <StateProperties />
        </Grid>
      </Grid>
    </Box>
  );
}
