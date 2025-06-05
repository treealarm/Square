/* eslint-disable no-unused-vars */
import { Button, Stack, Box, TextField, MenuItem, Select, InputLabel, FormControl } from "@mui/material";
import React, { useState } from "react";

interface TelemetryControlProps {
  moveMap: Record<string, string>;
  object_id: string;
  onControl: (direction: string, object_id: string, step: number, moveType: string) => void;
}

export const TelemetryControl: React.FC<TelemetryControlProps> = ({ moveMap, onControl, object_id }) => {
  const [moveType, setMoveType] = useState("relative");
  const [step, setStep] = useState(1);

  const [pan, setPan] = React.useState("0");
  const [tilt, setTilt] = React.useState("0");
  const [zoom, setZoom] = React.useState("0");

  const handleMove = (direction: string) => {
    // Clamp step for continuous move between -1.0 and 1.0
    const clampedStep =
      moveType === "continuous" ? Math.max(-1, Math.min(1, step)) : step;
    onControl(direction, object_id, clampedStep, moveType);
  };

  if (moveType === "absolute") {
    return (
      <Stack spacing={1} mt={2} alignItems="center" direction="row" gap={1}>
        <input
          type="number"
          value={pan}
          step="0.01"
          onChange={e => setPan(e.target.value)}
          placeholder="Pan"
          style={{ width: 60, fontSize: 12 }}
        />
        <input
          type="number"
          value={tilt}
          step="0.01"
          onChange={e => setTilt(e.target.value)}
          placeholder="Tilt"
          style={{ width: 60, fontSize: 12 }}
        />
        <input
          type="number"
          value={zoom}
          step="0.01"
          onChange={e => setZoom(e.target.value)}
          placeholder="Zoom"
          style={{ width: 60, fontSize: 12 }}
        />
        <Button
          size="small"
          variant="contained"
          onClick={() => onControl({ pan: pan, tilt: tilt, zoom: zoom }, object_id)}
        >
          Move
        </Button>
      </Stack>
    );
  }

  return (
    <Stack spacing={1} mt={2} alignItems="center">
      {/* Controls row: Move type & step */}
      <Box display="flex" gap={1} alignItems="center">
        <FormControl size="small">
          <InputLabel id="move-type-label">Type</InputLabel>
          <Select
            labelId="move-type-label"
            value={moveType}
            onChange={(e) => setMoveType(e.target.value)}
            size="small"
            label="Type"
          >
            <MenuItem value="relative">Relative</MenuItem>
            <MenuItem value="absolute">Absolute</MenuItem>
            <MenuItem value="continuous">Continuous</MenuItem>
          </Select>
        </FormControl>
        <TextField
          label="Step"
          type="number"
          value={step}
          onChange={(e) => setStep(parseFloat(e.target.value) || 0)}
          size="small"
          inputProps={{ step: "0.1", min: "-10", max: "10" }}
        />
      </Box>

      {/* Up */}
      {moveMap.up && <Button size="small" onClick={() => handleMove("up")}>↑</Button>}

      {/* Left / Right / Zooms */}
      <Box display="flex" gap={1}>
        {moveMap.left && <Button size="small" onClick={() => handleMove("left")}>←</Button>}
        {moveMap.zoom_in && <Button size="small" onClick={() => handleMove("zoom_in")}>＋</Button>}
        {moveMap.zoom_out && <Button size="small" onClick={() => handleMove("zoom_out")}>－</Button>}
        {moveMap.right && <Button size="small" onClick={() => handleMove("right")}>→</Button>}
      </Box>

      {/* Down */}
      {moveMap.down && <Button size="small" onClick={() => handleMove("down")}>↓</Button>}
    </Stack>
  );
};
