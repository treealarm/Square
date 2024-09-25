/* eslint-disable no-unused-vars */
import React, { useCallback } from 'react';
import { TextField, Box } from '@mui/material';

export interface IGeoExtraProperties {
  radius?: number;
  zoom_level?: string;
}

export interface IGeoExtraPropertiesEditorProps {
  extraProps: IGeoExtraProperties;
  handleChangeProp: (updatedProps: IGeoExtraProperties) => void;
  showRadius: boolean; // Флаг для отображения поля радиуса
}

const GeoExtraPropertiesEditor = ({ extraProps, handleChangeProp, showRadius }: IGeoExtraPropertiesEditorProps) => {
  const handleRadiusChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    handleChangeProp({
      ...extraProps,
      radius: parseFloat(event.target.value),
    });
  }, [extraProps, handleChangeProp]);

  const handleZoomLevelChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    handleChangeProp({
      ...extraProps,
      zoom_level: event.target.value,
    });
  }, [extraProps, handleChangeProp]);


  return (
    <Box display="flex" style={{ width: '100%' }} alignItems="center" gap={2} my={2}>
      {showRadius && (
        <TextField
          size="small"
          label="radius"
          type="number"
          value={extraProps.radius || ''}
          onChange={handleRadiusChange}
          fullWidth
        />
      )}
      <TextField
        size="small"
        label="zoom_level"
        value={extraProps.zoom_level || ''}
        onChange={handleZoomLevelChange}
        fullWidth
      />
    </Box>
  );
};

export default GeoExtraPropertiesEditor;
