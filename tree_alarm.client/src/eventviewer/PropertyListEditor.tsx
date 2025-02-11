/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import React, { useState, useEffect } from 'react';
import { Box, Button, TextField, IconButton, Tooltip, Popper, Paper } from '@mui/material';
import ClearOutlinedIcon from '@mui/icons-material/ClearOutlined';

interface PropertyListEditorProps {
  onChange: (data: string[]) => void;
  btn_text: string;
}

export const PropertyListEditor = ({ onChange, btn_text }: PropertyListEditorProps) => {
  const [items, setItems] = useState<string[]>(['']);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const handleChange = (index: number, value: string) => {
    let updatedItems = [...items];
    updatedItems[index] = value;

    // Удаляем пустые строки (кроме последней)
    updatedItems = updatedItems.filter((item, i) => item.trim() !== '' || i === updatedItems.length - 1);

    // Добавляем новую строку, если последняя заполнена
    if (updatedItems[updatedItems.length - 1].trim() !== '') {
      updatedItems.push('');
    }

    setItems(updatedItems);
    onChange(updatedItems.filter(item => item.trim() !== ''));
  };

  const deleteItem = (index: number) => {
    let updatedItems = items.filter((_, i) => i !== index);
    if (updatedItems.length === 0 || updatedItems[updatedItems.length - 1].trim() !== '') {
      updatedItems.push('');
    }
    setItems(updatedItems);
    onChange(updatedItems.filter(item => item.trim() !== ''));
  };

  const togglePopper = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(anchorEl ? null : event.currentTarget);
  };

  useEffect(() => {
    onChange(items.filter(item => item.trim() !== ''));
  }, []);

  return (
    <Box>
      <Tooltip title={items.filter(item => item.trim() !== '').join('\r\n')}>
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
          {items.length > 0 && (
            <Box display="flex" flexDirection="column" gap={2}>
              {items.map((item, index) => (
                <Box key={index} display="flex" alignItems="center" gap={2}>
                  <TextField
                    label="Value"
                    value={item}
                    onChange={(e) => handleChange(index, e.target.value)}
                    size="small"
                    fullWidth
                  />
                  <IconButton onClick={() => deleteItem(index)}>
                    <Tooltip title="Delete">
                      <ClearOutlinedIcon />
                    </Tooltip>
                  </IconButton>
                </Box>
              ))}
            </Box>
          )}
        </Paper>
      </Popper>
    </Box>
  );
};
