/* eslint-disable no-unused-vars */
import { Box, Button, TextField } from '@mui/material';
import React from 'react';
import { ICredentialListDTO } from '../store/Marker';

interface Props {
  value: ICredentialListDTO;
  onChange: (value: ICredentialListDTO) => void;
}

export const CredentialListInput: React.FC<Props> = ({ value, onChange }) => {
  const credentials = value?.credentials ?? [];

  const updateCredential = (index: number, field: 'username' | 'password', newVal: string) => {
    const updated = credentials.map((cred, i) =>
      i === index ? { ...cred, [field]: newVal } : cred
    );
    onChange({ credentials: updated });
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {credentials.map((cred, i) => (
        <Box key={i} sx={{ display: 'flex', gap: 1 }}>
          <TextField
            label={`Username ${i + 1}`}
            value={cred.username}
            onChange={(e) => updateCredential(i, 'username', e.target.value)}
            fullWidth
            size="small"
          />
          <TextField
            label={`Password ${i + 1}`}
            value={cred.password}
            onChange={(e) => updateCredential(i, 'password', e.target.value)}
            fullWidth
            size="small"
          />
        </Box>
      ))}
      <Button
        variant="outlined"
        onClick={() => onChange({ credentials: [...credentials, { username: '', password: '' }] })}
      >
        + Add Credential
      </Button>
    </Box>
  );
};
