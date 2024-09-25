/* eslint-disable no-unused-vars */
import React, { useCallback } from 'react';
import { TextField, Box } from '@mui/material';
import ZoomLevelSelector from './ZoomLevelSelector';

export interface IGeoExtraProperties {
  radius?: number;
  zoom_level?: string|null;
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

  const handleZoomLevelChange = useCallback((cur_level: string | null) => {
    handleChangeProp({
      ...extraProps,
      zoom_level: cur_level,
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
      <ZoomLevelSelector
        selectedZoomLevel={extraProps.zoom_level || ''}
        onZoomLevelChange={handleZoomLevelChange}
        fullWidth
      />
    </Box>
  );
};

export default GeoExtraPropertiesEditor;
