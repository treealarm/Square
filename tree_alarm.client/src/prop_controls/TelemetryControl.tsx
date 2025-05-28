/* eslint-disable no-unused-vars */
import { Button, Stack, Box } from "@mui/material";
import React from "react";

interface TelemetryControlProps {
  moveMap: Record<string, string>;
  object_id: string;
  onControl: (direction: string, object_id: string) => void;
}

export const TelemetryControl: React.FC<TelemetryControlProps> = ({ moveMap, onControl, object_id }) => {
  return (
    <Stack spacing={1} mt={2} alignItems="center">
      {moveMap.up && <Button onClick={() => onControl("up", object_id)}>↑</Button>}
      <Box display="flex" gap={1}>
        {moveMap.left && <Button onClick={() => onControl("left", object_id)}>←</Button>}
        {moveMap.right && <Button onClick={() => onControl("right", object_id)}>→</Button>}
      </Box>
      {moveMap.down && <Button onClick={() => onControl("down", object_id)}>↓</Button>}
    </Stack>
  );
};
