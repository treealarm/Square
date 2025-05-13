/* eslint-disable no-unused-vars */
import { TextField } from '@mui/material';
import React from 'react';

interface Props {
  label: string;
  type: 'text' | 'number';
  value: any;
  onChange: (value: any) => void;
}

export const GenericInput: React.FC<Props> = ({ label, type, value, onChange }) => (
  <TextField
    label={label}
    type={type}
    value={value ?? ''}
    onChange={(e) => onChange(type === 'number' ? Number(e.target.value) : e.target.value)}
    fullWidth
    size="small"
    margin="dense"
  />
);
