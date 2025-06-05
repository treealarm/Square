/* eslint-disable no-unused-vars */
import { Button, Stack, Box, TextField, MenuItem, Select, InputLabel, FormControl } from "@mui/material";
import React, { useState } from "react";

interface TelemetryControlProps {
  moveMap: Record<string, string>;
  object_id: string;
  onControl: (payload: Record<string, string>, object_id: string) => void;
}

export const TelemetryControl: React.FC<TelemetryControlProps> = ({ moveMap, onControl, object_id }) => {
  const [moveType, setMoveType] = useState("relative");
  const [step, setStep] = useState(1);
  const [pan, setPan] = useState("0");
  const [tilt, setTilt] = useState("0");
  const [zoom, setZoom] = useState("0");

  const handleMove = (param: string, value?: number) => {
    const clampedStep =
      moveType === "continuous"
        ? Math.max(-1, Math.min(1, value ?? step))
        : value ?? step;

    onControl(
      {
        [param]: clampedStep.toString(),
        move_type: moveType,
        speed: clampedStep.toString(),
      },
      object_id
    );
  };


  const handleAbsoluteMove = () => {
    onControl({
      pan: pan,
      tilt: tilt,
      zoom: zoom,
      move_type: "absolute",
      speed: "1", // можно заменить на отдельное поле speed, если нужно
    }, object_id);
  };

  return (
    <Stack spacing={2} mt={1} width="100%">
        <Stack width="100%" spacing={2}>
        
        <FormControl size="small" fullWidth>
          <Select
            labelId="move-type-label"
            value={moveType}
            onChange={(e) => setMoveType(e.target.value)}
            size="small"
            fullWidth
          >
            <MenuItem value="relative">Relative</MenuItem>
            <MenuItem value="absolute">Absolute</MenuItem>
            <MenuItem value="continuous">Continuous</MenuItem>
          </Select>
        </FormControl>
        {moveType !== "absolute" && (
          <TextField
            fullWidth
            label="Step"
            type="number"
            value={step}
            onChange={(e) => setStep(parseFloat(e.target.value) || 0)}
            size="small"
            inputProps={{ step: "0.1", min: "-10", max: "10" }}
          />
          )}
       
      </Stack>

      {moveType !== "absolute" && (
        
          <Stack width="100%" direction="row" spacing={2} justifyContent="center" alignItems="center">
        {/* Пан/Тилт */}
          <Stack spacing={1} alignItems="center" width="100%">
          <Button size="small" onClick={() => handleMove("tilt", step)}>↑</Button>
          <Stack direction="row" spacing={1}>
            <Button size="small" onClick={() => handleMove("pan", -step)}>←</Button>
            <Button size="small" onClick={() => handleMove("pan", step)}>→</Button>
          </Stack>
          <Button size="small" onClick={() => handleMove("tilt", -step)}>↓</Button>
        </Stack>

        {/* Зум вертикально */}
        <Stack spacing={1}>
          <Button size="small" onClick={() => handleMove("zoom", step)}>＋</Button>
          <Button size="small" onClick={() => handleMove("zoom", -step)}>－</Button>
        </Stack>
          </Stack>
      
        )}
      {/* Absolute inputs */}
      {moveType === "absolute" && (
        
          <Stack width="100%" spacing={2}>
          <TextField
            label="Pan"
            type="number"
            value={pan}
            onChange={(e) => setPan(e.target.value)}
            size="small"
            
          />
          <TextField
            label="Tilt"
            type="number"
            value={tilt}
            onChange={(e) => setTilt(e.target.value)}
            size="small"
            
          />
          <TextField
            label="Zoom"
            type="number"
            value={zoom}
            onChange={(e) => setZoom(e.target.value)}
            size="small"
            
          />
          <Button
            variant="contained"
            size="small"
            onClick={handleAbsoluteMove}
            
          >
            Move
          </Button>
          </Stack>
       
      )}
    </Stack>

  );

};
