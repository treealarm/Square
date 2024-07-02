import * as React from 'react';
import { TextField, Button, Box, Typography, InputAdornment, IconButton } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import { IControlSelector } from 'control_selector_common';

const GeoEditor = ({ props }: { props: IControlSelector }) => {
  const [geometry, setGeometry] = React.useState(JSON.parse(props.str_val));
  const [editMode, setEditMode] = React.useState(false);

  // Handle coordinate change
  const handleCoordChange = (index: number, lat: number, lng: number) => {
    const newCoords = Array.isArray(geometry.coord[0])
      ? geometry.coord.map((coord: [number, number], i: number) =>
        i === index ? [lat, lng] : coord
      )
      : [lat, lng];
    setGeometry({ ...geometry, coord: newCoords });
    props.handleChangeProp({ target: { value: JSON.stringify({ ...geometry, coord: newCoords }) } });
  };

  // Handle adding a new coordinate (for lines and polygons)
  const handleAddCoord = () => {
    if (geometry.type !== 'Point') {
      const newCoords = [...geometry.coord, [0, 0]];
      setGeometry({ ...geometry, coord: newCoords });
      props.handleChangeProp({ target: { value: JSON.stringify({ ...geometry, coord: newCoords }) } });
    }
  };

  // Handle removing a coordinate (for lines and polygons)
  const handleRemoveCoord = (index: number) => {
    if (geometry.type !== 'Point') {
      const newCoords = geometry.coord.filter((_: any, i: number) => i !== index);
      setGeometry({ ...geometry, coord: newCoords });
      props.handleChangeProp({ target: { value: JSON.stringify({ ...geometry, coord: newCoords }) } });
    }
  };

  const renderCoords = () => {
    if (geometry.type === 'Point') {
      return (
        <CoordInput
          index={0}
          lat={geometry.coord[0]}
          lng={geometry.coord[1]}
          onCoordChange={handleCoordChange}
          onRemoveCoord={null}
        />
      );
    }
    return geometry.coord.map((coord: [number, number], index: number) => (
      <CoordInput
        key={index}
        index={index}
        lat={coord[0]}
        lng={coord[1]}
        onCoordChange={handleCoordChange}
        onRemoveCoord={handleRemoveCoord}
      />
    ));
  };

  return (
    <Box>
      <TextField
        size="small"
        fullWidth
        id={props.prop_name}
        label={props.prop_name}
        value={props.str_val}
        InputProps={{
          readOnly: true,
          endAdornment: (
            <InputAdornment position="end">
              <IconButton
                edge="end"
                color="primary"
                onClick={() => setEditMode(!editMode)}
              >
                <EditIcon />
              </IconButton>
            </InputAdornment>
          )
        }}
      />
      {editMode && (
        <Box mt={2}>
          <Typography variant="h10">
            Editing {geometry.type === 'Polygon' ? 'Polygon' : geometry.type === 'LineString' ? 'LineString' : 'Point'}
          </Typography>
          {renderCoords()}
          {geometry.type !== 'Point' && (
            <Button variant="contained" color="primary" onClick={handleAddCoord}>
              Add Coordinate
            </Button>
          )}
        </Box>
      )}
    </Box>
  );
};

// Coordinate input component
const CoordInput = ({ index, lat, lng, onCoordChange, onRemoveCoord }: any) => {
  const handleLatChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newLat = parseFloat(e.target.value);
    onCoordChange(index, newLat, lng);
  };

  const handleLngChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newLng = parseFloat(e.target.value);
    onCoordChange(index, lat, newLng);
  };

  return (
    <Box display="flex" alignItems="center" my={1}>
      <TextField
        size="small"
        label={`Latitude ${index + 1}`}
        type="number"
        value={lat}
        onChange={handleLatChange}
        style={{ marginRight: 8 }}
      />
      <TextField
        size="small"
        label={`Longitude ${index + 1}`}
        type="number"
        value={lng}
        onChange={handleLngChange}
        style={{ marginRight: 8 }}
      />
      {onRemoveCoord && (
        <Button variant="contained" color="secondary" onClick={() => onRemoveCoord(index)}>
          Remove
        </Button>
      )}
    </Box>
  );
};

export default GeoEditor;
