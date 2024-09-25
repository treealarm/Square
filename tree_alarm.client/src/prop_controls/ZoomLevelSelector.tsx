/* eslint-disable no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useAppDispatch } from '../store/configureStore';
import { ApplicationState } from '../store';
import * as ZoomLevelsStore from '../store/ZoomLevelsStates';
import { Autocomplete, TextField, CircularProgress } from '@mui/material';
import { ZoomLevelsState } from '../store/ZoomLevelsStates';

interface ZoomLevelSelectorProps {
  onZoomLevelChange: (cur_level: string | null) => void;
  selectedZoomLevel: string | null; // To hold the selected zoom level
}

const ZoomLevelSelector = (props: ZoomLevelSelectorProps) => {
  const { onZoomLevelChange, selectedZoomLevel } = props; // Destructure props
  const appDispatch = useAppDispatch();

  // Extracting state from Redux
  const zoom_state: ZoomLevelsState | null = useSelector((state: ApplicationState) => state.zoomLevelsStates ?? null);

  // Fetching zoom levels on component mount
  useEffect(() => {
    appDispatch(ZoomLevelsStore.fetchZoomLevels());
  }, []);

  return (
    <Autocomplete
      fullWidth
      size="small"
      options={zoom_state?.levels || []}
      getOptionLabel={(option) => option.zoom_level || ''}
      onChange={(event, newValue) => {
        onZoomLevelChange(newValue ? newValue.zoom_level : null);
      }}
      loading={zoom_state?.isLoading}
      value={zoom_state?.levels.find(level => level.zoom_level === selectedZoomLevel) || null}
      disableClearable
      renderInput={(params) => (
        <TextField
          {...params}
          label="zoom_level"
          variant="outlined"
          InputProps={{
            ...params.InputProps,
            endAdornment: (
              <>
                {zoom_state?.isLoading ? <CircularProgress color="inherit" size={20} /> : null}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
        />
      )}
      noOptionsText={zoom_state?.error ? `Error: ${zoom_state.error}` : 'No available zoom levels'}
    />
  );
};

export default ZoomLevelSelector;
