/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import React, { useState, useEffect } from 'react';
import { Box, Button, TextField, IconButton, Tooltip, Popper, Paper } from '@mui/material';
import ClearOutlinedIcon from '@mui/icons-material/ClearOutlined';
import { KeyValueDTO, ObjPropsSearchDTO } from '../store/Marker';

interface PropertyFilterEditorProps {
  onChange: (data: ObjPropsSearchDTO) => void;
}

export const PropertyFilterEditor = ({ onChange }: PropertyFilterEditorProps) => {
  const [keyValuePairs, setKeyValuePairs] = useState<KeyValueDTO[]>([{ str_val: '', prop_name: '' }]);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const handleChange = (index: number, field: 'prop_name' | 'str_val', value: string) => {
    let updatedPairs = [...keyValuePairs];
    updatedPairs[index] = { ...updatedPairs[index], [field]: value };

    // ���� ������ `prop_name` � �� ���������� ������, ������� ������ (���� ��� �� ���������)
    if (field === 'prop_name' && value.trim() === '') {
      updatedPairs = updatedPairs.filter((_, i) => i !== index);
    }

    // ��������� ����� ������, ���� ��������� ���������
    if (updatedPairs.length === 0 || updatedPairs[updatedPairs.length - 1].prop_name.trim() !== '') {
      updatedPairs.push({ prop_name: '', str_val: '' });
    }

    setKeyValuePairs(updatedPairs);

    // �������� ����������� ������ ��� ������ ������
    onChange({ props: updatedPairs.filter(pair => pair.prop_name.trim() !== '') });
  };

  const deleteKeyValuePair = (index: number) => {
    let updatedPairs = keyValuePairs.filter((_, i) => i !== index);

    // ���� ������� ��������� ������, ��������� ����� ������ ������
    if (updatedPairs.length === 0 || updatedPairs.some(pair => pair.prop_name.trim() !== '')) {
      updatedPairs.push({ prop_name: '', str_val: '' });
    }

    setKeyValuePairs(updatedPairs);

    // �������� ����������� ������ ��� ������ ������
    onChange({ props: updatedPairs.filter(pair => pair.prop_name.trim() !== '') });
  };


  // ��������/�������� ������������ ����
  const togglePopper = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(anchorEl ? null : event.currentTarget);
  };

  useEffect(() => {
    // ���������� �������� ������ ������, ���� � ������ ������ ������
    onChange({ props: keyValuePairs.filter(pair => pair.prop_name.trim() !== '') });
  }, []); // ����������� ������ ���� ��� ��� ������������ ����������

  return (
    <Box>
      {/* ������ ��� �������� ������������ ���� */}
      <Tooltip title={keyValuePairs.map((pair) => `${pair.prop_name}: ${pair.str_val}`).join('\r\n')}>
        <Button onClick={togglePopper} variant="outlined">
          Open Property Filter
        </Button>
      </Tooltip>

      {/* Popper ��� ����������� ������ */}
      <Popper
        open={Boolean(anchorEl)}
        anchorEl={anchorEl}
        placement="bottom-start"
        style={{ zIndex: 1300 }}
      >
        <Paper sx={{ padding: 2, minWidth: 300 }}>
          {/* ������ ������-�������� */}
          {keyValuePairs.length > 0 && (
            <Box display="flex" flexDirection="column" gap={2}>
              {keyValuePairs.map((pair, index) => (
                <Box key={index} display="flex" alignItems="center" gap={2}>
                  <TextField
                    label="Property Name"
                    value={pair.prop_name}
                    onChange={(e) => handleChange(index, 'prop_name', e.target.value)}
                    size="small"
                    fullWidth
                  />
                  <TextField
                    label="Value"
                    value={pair.str_val}
                    onChange={(e) => handleChange(index, 'str_val', e.target.value)}
                    size="small"
                    fullWidth
                  />
                  <IconButton onClick={() => deleteKeyValuePair(index)}>
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
