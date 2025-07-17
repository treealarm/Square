/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import { Box, Button, TextField, Popper, Paper, Tooltip } from '@mui/material';


interface PropertyFilterEditorProps {
  onChange: (data: { param0: string | null; param1: string | null }) => void;
  btn_text: string;
}

export const PropertyFilterEditor = ({ onChange, btn_text }: PropertyFilterEditorProps) => {
  const [param0, setParam0] = useState<string>('');
  const [param1, setParam1] = useState<string>('');
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const togglePopper = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(anchorEl ? null : event.currentTarget);
  };

  const handleParam0Change = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setParam0(value);
    onChange({ param0: value || null, param1: param1 || null });
  };

  const handleParam1Change = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setParam1(value);
    onChange({ param0: param0 || null, param1: value || null });
  };

  return (
    <Box>
      <Tooltip title={`param0: ${param0 || '—'}\nparam1: ${param1 || '—'}`}>
        <Button onClick={togglePopper} variant="outlined">
          {btn_text}
        </Button>
      </Tooltip>

      <Popper
        open={Boolean(anchorEl)}
        anchorEl={anchorEl}
        placement="bottom-start"
        style={{ zIndex: 1300 }}
      >
        <Paper sx={{ padding: 2, minWidth: 300 }}>
          <Box display="flex" flexDirection="column" gap={2}>
            <TextField
              label="Param 0"
              value={param0}
              onChange={handleParam0Change}
              size="small"
              fullWidth
            />
            <TextField
              label="Param 1"
              value={param1}
              onChange={handleParam1Change}
              size="small"
              fullWidth
            />
          </Box>
        </Paper>
      </Popper>
    </Box>
  );
};
