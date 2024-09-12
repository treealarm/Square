/* eslint-disable no-unused-vars */
import * as React from 'react';
import { TextField, Box, IconButton, Button } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import { CoordSelector } from '../components/CoordSelector';

interface CoordInputProps {
  index: number;
  lat: number;
  lng: number;
  onCoordChange: (index: number, lat: number, lng: number) => void;
  onRemoveCoord?: (index: number) => void;
}

const CoordInput = ({ index, lat, lng, onCoordChange, onRemoveCoord }: CoordInputProps) => {

  const handleLatChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newLat = parseFloat(e.target.value);
    onCoordChange(index, newLat, lng);
  };

  const handleLngChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newLng = parseFloat(e.target.value);
    onCoordChange(index, lat, newLng);
  };

  const handleMapSelection = (selectedLat: number, selectedLng: number) => {
    onCoordChange(index, selectedLat, selectedLng); // Изменение координат на основе выбора на карте
  };

  return (
    <Box display="flex" alignItems="center" my={1}>
      <TextField
        size="small"
        label={`Lat ${index}`}
        type="number"
        value={lat}
        onChange={handleLatChange}
        style={{ marginRight: 8, marginTop: 4 }}
        inputProps={{ style: { fontSize: '0.875rem' } }}
      />
      <TextField
        size="small"
        label={`Lng ${index}`}
        type="number"
        value={lng}
        onChange={handleLngChange}
        style={{ marginRight: 8, marginTop: 4 }}
        inputProps={{ style: { fontSize: '0.875rem' } }}
      />
      {onRemoveCoord && (
        <IconButton
          edge="end"
          color="secondary"
          onClick={() => onRemoveCoord(index)}
        >
          <DeleteIcon fontSize="small" />
        </IconButton>
      )}

        <CoordSelector
          lat={lat}  // Передаем текущие координаты
          lon={lng}
          onConfirm={(newLat, newLng) => {
            handleMapSelection(newLat, newLng); // Обработка выбранных на карте координат
          }}
        />
    </Box>
  );
};

export default CoordInput;
