/* eslint-disable no-unused-vars */
import { Box, TextField } from '@mui/material';
import React from 'react';
import { IIpRangeDTO } from '../store/Marker';

interface Props {
  value: IIpRangeDTO;
  onChange: (value: IIpRangeDTO) => void;
  label?: string;
}

export const IpRangeInput: React.FC<Props> = ({ value, onChange, label }) => (
  <Box sx={{ display: 'flex', gap: 1 }}>
    <TextField
      label={`${label ?? 'IP Range'} - Start IP`}
      value={value?.start_ip ?? ''}
      onChange={(e) => onChange({ ...value, start_ip: e.target.value })}
      fullWidth
      size="small"
    />
    <TextField
      label="End IP"
      value={value?.end_ip ?? ''}
      onChange={(e) => onChange({ ...value, end_ip: e.target.value })}
      fullWidth
      size="small"
    />
  </Box>
);
