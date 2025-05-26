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
} from "@mui/material";
import { Visibility } from "@mui/icons-material";
import { IControlSelector } from "./control_selector_common";

export function renderSnapshotViewer(props: IControlSelector) {
  const [open, setOpen] = useState(false);
  const [imageSrc, setImageSrc] = useState("");
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  const buildUrl = () => `${props.str_val}?t=${Date.now()}`;

  useEffect(() => {
    if (open) {
      const updateImage = () => setImageSrc(buildUrl());
      updateImage();
      intervalRef.current = setInterval(updateImage, 5000);
    }
    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [open]);

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
        <DialogTitle>
          Snapshot Viewer
        </DialogTitle>
        <DialogContent dividers>
          {imageSrc ? (
            <Box
              display="flex"
              justifyContent="center"
              alignItems="center"
              width="100%"
              height="100%"
            >
              <img
                src={imageSrc}
                alt="Snapshot"
                style={{ maxWidth: "100%", maxHeight: "60vh" }}
              />
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
