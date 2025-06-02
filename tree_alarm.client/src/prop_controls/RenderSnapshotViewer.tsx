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

export function renderSnapshotViewer(props: IControlSelector) {
  const [open, setOpen] = useState(false);
  const [imageSrc, setImageSrc] = useState("");
  const [objectActions, setObjectActions] = useState<IActionDescrDTO[]>([]);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  const buildUrl = () => `${props.str_val}?t=${Date.now()}`;

  const appDispatch = useAppDispatch();

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

  useEffect(() => {
    if (!open || !props.object_id) return;

    // Загружаем действия — один раз
    fetchAvailableActionsRaw(props.object_id)
      .then(setObjectActions)
      .catch(() => setObjectActions([]));

    // Обновляем скриншот
    const update = () => {
      setImageSrc(buildUrl());            // обновим картинку
    };

    update(); // сразу
    intervalRef.current = setInterval(update, 5000); // периодически

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [open, props.object_id, refreshSnapshot]); // 👈 добавь object_id в зависимости!




  const handleTelemetryControl = (direction: string, object_id:string) => {
    const action = objectActions.find(a => a.name === "telemetry");
    if (!action || !object_id) return;

    // Формируем параметр с направлением движения
    const telemetryParam = {
      name: "move",
      type: "__map", // либо нужный тип для мапы
      cur_val: {
        [direction]: "1"
      }
    };

    // Создаем объект IActionExeDTO для вызова executeAction
    const actionExePayload: IActionExeDTO = {
      object_id: object_id,
      name: action.name!,
      uid: null,
      parameters: [telemetryParam]
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
              justifyContent="center"
              alignItems="center"
              width="100%"
              height="100%"
              flexDirection="column"
            >
              <img
                src={imageSrc}
                alt="Snapshot"
                style={{ maxWidth: "100%", maxHeight: "60vh" }}
              />
              {moveMap && (
                <TelemetryControl moveMap={moveMap} onControl={handleTelemetryControl} object_id={props.object_id} />
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
            Обновить
          </Button>
          <Button onClick={() => setOpen(false)}>Close</Button>
        </DialogActions>

      </Dialog>
    </>
  );
}

