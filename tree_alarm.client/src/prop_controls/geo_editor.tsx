/* eslint-disable react/prop-types */
import * as React from 'react';
import { TextField, Button, Box, Typography, InputAdornment, IconButton } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import { IControlGeoProps } from './control_selector_common';
import CoordInput from './CoordInput';
import { IPointCoord, IPolygonCoord, IPolylineCoord } from '../store/Marker';

const defaultGeometry: IPointCoord = {
  type: 'Point' as const,
  coord: [0, 0]
};

const GeoEditor = ({ props }: { props: IControlGeoProps }) => {
  const [geometry, setGeometry] = React.useState<IPointCoord | IPolygonCoord | IPolylineCoord>(defaultGeometry);
  const [editMode, setEditMode] = React.useState(false);

  React.useEffect(() => {
    try {
      const parsedGeometry = props.val;// JSON.parse(props.str_val);
      if (parsedGeometry && ['Point', 'Polygon', 'LineString'].includes(parsedGeometry.type)) {
        setGeometry(parsedGeometry);
      } else {
        setGeometry(defaultGeometry);
      }
    } catch (error) {
      // JSON parsing failed or data is invalid
      setGeometry(defaultGeometry);
    }
  }, [props.val]);

  const handleChange = (newValue: string) => {
    // ׁמחהאול סמבûעטו, סמגלוסעטלמו ס handleChangeProp
    const event = {
      target: {
        id: props.prop_name,
        value: newValue
      }
    };
    props.handleChangeProp(event);
  };

  // Handle coordinate change
  const handleCoordChange = (index: number, lat: number, lng: number) => {
    const newCoords = Array.isArray(geometry.coord[0])
      ? geometry.coord.map((coord: [number, number], i: number) =>
        i === index ? [lat, lng] : coord
      )
      : [lat, lng];
    setGeometry({ ...geometry, coord: newCoords });
    handleChange(JSON.stringify({ ...geometry, coord: newCoords }));
  };

  // Handle adding a new coordinate (for lines and polygons)
  const handleAddCoord = () => {
    if (geometry.type !== 'Point') {
      const newCoords = [...geometry.coord, [0, 0]];
      setGeometry({ ...geometry, coord: newCoords });
      handleChange(JSON.stringify({ ...geometry, coord: newCoords }));
    }
  };

  // Handle removing a coordinate (for lines and polygons)
  const handleRemoveCoord = (index: number) => {
    if (geometry.type !== 'Point') {
      const newCoords = geometry.coord.filter((_: any, i: number) => i !== index);
      setGeometry({ ...geometry, coord: newCoords });
      handleChange(JSON.stringify({ ...geometry, coord: newCoords }));
    }
  };

  const renderCoords = () => {
    if (geometry.type === 'Point') {
      return (
        <CoordInput
          index={0}
          lat={geometry?.coord[0]??0}
          lng={geometry?.coord[1]??0}
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
    <Box width="100%">
      <TextField
        size="small"
        fullWidth
        id={props.prop_name}
        label={props.prop_name}
        value={JSON.stringify(props.val)}
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
          ),
        }}
      />
      {editMode && (
        <Box mt={2}>
          <Typography variant="h6">
            Editing {geometry.type === 'Polygon' ? 'Polygon' : geometry.type === 'LineString' ? 'LineString' : 'Point'}
          </Typography>
          {renderCoords()}
          {geometry.type !== 'Point' && (
            <Button variant="contained" color="primary" onClick={handleAddCoord}>
              Add Coord
            </Button>
          )}
        </Box>
      )}
    </Box>
  );
};

export default GeoEditor;
