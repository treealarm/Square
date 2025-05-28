/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */
/* eslint-disable react-hooks/rules-of-hooks */
import React, { useEffect, useRef, useState } from "react";
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
import { IActionDescrDTO } from "../store/Marker";
import { fetchAvailableActionsRaw } from "../store/IntegroStates";

export function renderSnapshotViewer(props: IControlSelector) {
  const [open, setOpen] = useState(false);
  const [imageSrc, setImageSrc] = useState("");
  const [telemetryActions, setTelemetryActions] = useState<IActionDescrDTO[]>([]);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  const buildUrl = () => `${props.str_val}?t=${Date.now()}`;

  useEffect(() => {
    if (open) {
      const updateImage = () => setImageSrc(buildUrl());
      updateImage();
      intervalRef.current = setInterval(updateImage, 5000);

      if (props.object_id == null) {
        return;
      }
      // Загрузка телеметрии
      fetchAvailableActionsRaw(
        props.object_id
      )
        .then(setTelemetryActions)
        .catch(() => setTelemetryActions([])); // Если ошибка — просто не показываем

    }
    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [open]);

  const handleTelemetryControl = (direction: string) => {
    const action = telemetryActions.find(a => a.name === "telemetry");
    if (action) {
      console.log(`Send telemetry command '${direction}' to action id: ${action.uid}`);
      // Тут вызов функции выполнения действия, например: executeAction(action.uid, direction)
    }
  };

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
              {telemetryActions.length > 0 && (
                <Stack spacing={1} mt={2} alignItems="center">
                  <Button onClick={() => handleTelemetryControl("up")}>↑</Button>
                  <Box display="flex" gap={1}>
                    <Button onClick={() => handleTelemetryControl("left")}>←</Button>
                    <Button onClick={() => handleTelemetryControl("right")}>→</Button>
                  </Box>
                  <Button onClick={() => handleTelemetryControl("down")}>↓</Button>
                </Stack>
              )}
            </Box>
          ) : (
            <Typography>No image available</Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}

