/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */
/* eslint-disable react-hooks/rules-of-hooks */
import React, { useCallback, useEffect, useRef, useState } from "react";
import {
  Box,
  TextField,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Stack,
} from "@mui/material";
import { Visibility } from "@mui/icons-material";
import { IControlSelector } from "./control_selector_common";
import { IActionDescrDTO, IActionExeDTO } from "../store/Marker";
import { fetchAvailableActionsRaw } from "../store/IntegroStates";
import { TelemetryControl } from "./TelemetryControl";
import { useAppDispatch } from "../store/configureStore";
import * as IntegroStore from '../store/IntegroStates';
import { ApplicationState } from "../store";
import { useSelector } from "react-redux";

export function renderSnapshotViewer(props: IControlSelector) {
  const [open, setOpen] = useState(false);
  const [imageSrc, setImageSrc] = useState(props.str_val);
  const [objectActions, setObjectActions] = useState<IActionDescrDTO[]>([]);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  const buildUrl = () => `${props.str_val}?t=${Date.now()}`;

  const appDispatch = useAppDispatch();

  const snapshot: string | null = useSelector((state: ApplicationState) =>
    props.object_id ? state?.integroStates?.snapshots?.[props.object_id] ?? null : null
  );


  const refreshSnapshot = useCallback((object_id: string) => {
    const action = objectActions.find(a => a.name === "refresh");
    if (!action || !object_id) return;

    const actionExePayload: IActionExeDTO = {
      object_id,
      name: action.name!,
      uid: null,
      parameters: []
    };

    appDispatch(IntegroStore.executeAction(actionExePayload));
  }, [objectActions]);

  // Загружаем действия один раз при открытии
  useEffect(() => {
    if (!open || !props.object_id) return;

    fetchAvailableActionsRaw(props.object_id)
      .then(setObjectActions)
      .catch(() => setObjectActions([]));
  }, [open, props.object_id]);

  useEffect(() => {
    if (!open || !props.object_id) return;

    let cancelled = false;
    setImageSrc(buildUrl());
    const update = async () => {
      if (cancelled) return;

      console.log("getSnapshot");

      try {
        await appDispatch(IntegroStore.fetchSnapshot(props.object_id!));
        setImageSrc(buildUrl());
      } catch (e) {
        console.error("Failed to fetch snapshot", e);
      }

      if (!cancelled) {
        setTimeout(update, 1000);
      }
    };

    update();

    return () => {
      cancelled = true;
    };
  }, [open, props.object_id]);




  type TelemetryParams = {
    [key: string]: string;
  };

  const handleTelemetryControl = (
    payload: TelemetryParams,
    object_id: string
  ) => {
    const action = objectActions.find(a => a.name === "telemetry");
    if (!action || !object_id) return;

    const telemetryParam = {
      name: "move",
      type: "__map",
      cur_val: payload,
    };

    const actionExePayload: IActionExeDTO = {
      object_id,
      name: action.name!,
      uid: null,
      parameters: [telemetryParam],
    };

    appDispatch(IntegroStore.executeAction(actionExePayload));
  };


  const moveMap = (() => {
    const telemetry = objectActions.find(a => a.name === "telemetry");
    if (!telemetry) return undefined;

    const moveParam = telemetry.parameters?.find(
      (p: any) =>
        p.name === "move" &&
        p.type === "__map" &&
        typeof p.cur_val === "object" &&
        !Array.isArray(p.cur_val)
    );

    return moveParam?.cur_val as Record<string, string> | undefined;
  })();



  return (
    <>
      <TextField
        fullWidth
        size="small"
        label={props.prop_name}
        value={props.str_val}
        InputProps={{
          readOnly: true,
          endAdornment: (
            <IconButton onClick={() => setOpen(true)} edge="end">
              <Visibility />
            </IconButton>
          ),
        }}
      />

      <Dialog
        open={open}
        onClose={() => setOpen(false)}
        maxWidth="lg"
        fullWidth
        PaperProps={{
          sx: {
            resize: "both",
            overflow: "auto",
            minWidth: 300,
            minHeight: 200,
          },
        }}
      >
        <DialogTitle>Snapshot Viewer</DialogTitle>
        <DialogContent dividers>
          {imageSrc ? (
            <Box
              display="flex"
              flexDirection="row"
              width="100%"
              height="100%"
            >
              {/* Левая колонка: картинка по центру */}
              <Box
                display="flex"
                justifyContent="center"
                alignItems="center"
                flex="1"
              >
                <img
                  src={snapshot ? snapshot : imageSrc}
                  alt="Snapshot"
                  style={{ maxWidth: "100%", maxHeight: "60vh" }}
                />
              </Box>

              {/* Правая колонка: TelemetryControl вверху справа */}
              {moveMap && (
                <Box
                  display="flex"
                  justifyContent="flex-end"
                  alignItems="flex-start"
                  p={1}
                  width="150px" 
                >
                  <TelemetryControl
                    moveMap={moveMap}
                    onControl={handleTelemetryControl}
                    object_id={props.object_id ?? null}
                  />
                </Box>
              )}
            </Box>
          ) : (
            <Typography>No image available</Typography>
          )}
        </DialogContent>

        <DialogActions>
          <Button
            onClick={() => props.object_id && refreshSnapshot(props.object_id)}
            variant="outlined"
          >
            Refresh
          </Button>
          <Button onClick={() => setOpen(false)}>Close</Button>
        </DialogActions>

      </Dialog>
    </>
  );
}

