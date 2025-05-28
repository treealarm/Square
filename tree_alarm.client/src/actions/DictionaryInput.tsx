/* eslint-disable no-unused-vars */
import React from 'react';
import {
  Box,
  IconButton,
  TextField,
  Typography,
  Button,
  Grid,
} from '@mui/material';
import { Add, Delete } from '@mui/icons-material';

interface DictionaryProps {
  label: string;
  value: Record<string, string>; // map<string, string>
  onChange: (value: Record<string, string>) => void;
}

export function DictionaryInput({ label, value, onChange }: DictionaryProps) {
  const [entries, setEntries] = React.useState<[string, string][]>(Object.entries(value));

  const handleEntryChange = (index: number, key: string, val: string) => {
    const updated = [...entries];
    updated[index] = [key, val];
    setEntries(updated);
    onChange(Object.fromEntries(updated.filter(([k]) => k.trim() !== '')));
  };

  const handleAdd = () => {
    setEntries([...entries, ['', '']]);
  };

  const handleRemove = (index: number) => {
    const updated = entries.filter((_, i) => i !== index);
    setEntries(updated);
    onChange(Object.fromEntries(updated));
  };

  return (
    <Box>
      <Typography variant="subtitle1" gutterBottom>
        {label}
      </Typography>
      <Grid container spacing={1}>
        {entries.map(([k, v], i) => (
          <React.Fragment key={i}>
            <Grid item xs={5}>
              <TextField
                size="small"
                fullWidth
                label="Key"
                value={k}
                onChange={(e) => handleEntryChange(i, e.target.value, v)}
              />
            </Grid>
            <Grid item xs={5}>
              <TextField
                size="small"
                fullWidth
                label="Value"
                value={v}
                onChange={(e) => handleEntryChange(i, k, e.target.value)}
              />
            </Grid>
            <Grid item xs={2}>
              <IconButton onClick={() => handleRemove(i)}>
                <Delete />
              </IconButton>
            </Grid>
          </React.Fragment>
        ))}
      </Grid>
      <Box mt={1}>
        <Button startIcon={<Add />} onClick={handleAdd}>
          Add Pair
        </Button>
      </Box>
    </Box>
  );
}
