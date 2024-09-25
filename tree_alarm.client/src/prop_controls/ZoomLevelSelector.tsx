/* eslint-disable no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useAppDispatch } from '../store/configureStore';
import { ApplicationState } from '../store';
import * as ZoomLevelsStore from '../store/ZoomLevelsStates';
import { FormControl, InputLabel, Select, MenuItem, CircularProgress } from '@mui/material';
import { ZoomLevelsState } from '../store/ZoomLevelsStates';

interface ZoomLevelSelectorProps {
  onZoomLevelChange: (cur_level: string | null) => void;
  selectedZoomLevel: string | null; // Для хранения выбранного уровня зума
}

const ZoomLevelSelector: React.FC<ZoomLevelSelectorProps> = ({ onZoomLevelChange, selectedZoomLevel }) => {
  const appDispatch = useAppDispatch();

  // Извлечение состояния из Redux
  const zoom_state: ZoomLevelsState|null = useSelector((state: ApplicationState) => state.zoomLevelsStates??null);

  // Запрос уровней зума при монтировании компонента
  useEffect(() => {
    appDispatch(ZoomLevelsStore.fetchZoomLevels());
  }, [appDispatch]);

  // Обработка изменения выбранного уровня зума
  function handleChange(e: any) {
    const { target: { value } } = e;
    onZoomLevelChange(value as string || null);
  }

  return (
    <FormControl fullWidth>
      <InputLabel id="zoom_level_label">zoom_level</InputLabel>
      <Select
        size="small"
        labelId="zoom-level-label"
        id="zoom-level"
        value={selectedZoomLevel || ''}
        onChange={handleChange}
        label={"zoom_level" }
      >
        <MenuItem value="">
          {""}
        </MenuItem>
        {zoom_state?.isLoading && (
          <MenuItem disabled>
            <CircularProgress size={24} />
          </MenuItem>
        )}
        {zoom_state?.error && (
          <MenuItem disabled>
            Ошибка: {zoom_state.error}
          </MenuItem>
        )}
        {zoom_state?.levels?.map((level) => (
          <MenuItem key={level.id} value={level.zoom_level || ''}>
            {level.zoom_level}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
};

export default ZoomLevelSelector;
