/* eslint-disable react-hooks/rules-of-hooks */
/* eslint-disable no-undef */
/* eslint-disable react-hooks/exhaustive-deps */
import React, { useState, useEffect, useRef } from "react";
import { Button, Popover, Box } from "@mui/material";
import { IControlSelector } from "./control_selector_common";

export function renderSnapshotViewer(props: IControlSelector) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [imageSrc, setImageSrc] = useState("");
  const intervalRef = useRef<NodeJS.Timeout | null>(null);


  const buildUrl = () => `${props.str_val}?t=${Date.now()}`;

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const open = Boolean(anchorEl);

  useEffect(() => {
    if (open) {
      const updateImage = () => setImageSrc(buildUrl());
      updateImage();
      intervalRef.current = setInterval(updateImage, 5000);
      return () => {
        if (intervalRef.current) {
          clearInterval(intervalRef.current);
          intervalRef.current = null;
        }
      };
    } else {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    }
  }, [open]);

  return (
    <>
      <Button variant="outlined" size="small" onClick={handleOpen}>
        View
      </Button>
      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
        transformOrigin={{ vertical: 'top', horizontal: 'left' }}
      >
        <Box p={1}>
          <img
            src={imageSrc}
            alt="Snapshot"
            style={{ maxWidth: 400, maxHeight: 300, borderRadius: 4 }}
          />
        </Box>
      </Popover>
    </>
  );
}
