/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import React, { useCallback, useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import { GeometryType, ICircle, ICommonFig, IFigures, IObjProps, IPointCoord, IPolygonCoord, IPolylineCoord, LatLngPair, LineStringType, PointType, PolygonType } from '../store/Marker';
import GeoEditor from '../prop_controls/geo_editor';
import { useAppDispatch } from '../store/configureStore';
import * as MarkersStore from '../store/MarkersStates';
import { Box, Button, FormControl, InputLabel, MenuItem, Select } from '@mui/material';
import GeoExtraPropertiesEditor from '../prop_controls/GeoExtraPropertiesEditor';

export interface ChildEvents {
  clickSave: () => void;
}

// Интерфейс для props дочернего компонента
export interface IGeoEditorProperties {
  events: ChildEvents;
}

const SelectedObjectGeoEditor = (props: IGeoEditorProperties) => {
  const appDispatch = useAppDispatch();
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const selectedFig = useSelector((state: ApplicationState) => state?.markersStates?.selected_marker);
  const objProps: IObjProps | null = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps ?? null);

  useEffect(() => {
    if (selected_id) {
      if (selectedFig?.id != selected_id) {
        // Fetch the figure if it's not already set
        const fetchData = async () => {
          try {
            const fetchedFigures = await MarkersStore.requestMarkersByIds([selected_id]); // Ждем результат запроса
            appDispatch(MarkersStore.selectMarkerLocally(fetchedFigures.figs[0])); // Устанавливаем первую фигуру
          } catch (error) {
            console.error("Failed to fetch marker:", error);
            appDispatch(MarkersStore.selectMarkerLocally(null)); // Устанавливаем null в случае ошибки
          }
        };

        fetchData();
      }      
    } else {
      appDispatch(MarkersStore.selectMarkerLocally(null));
    }
  }, [selected_id, selectedFig]);

  const handleSave = useCallback(() => {
    if (selectedFig) {
      const figures: IFigures = {
        figs: [selectedFig],  // Сразу задаем figs
      };
      appDispatch(MarkersStore.updateMarkers(figures));  // Обновляем фигуру на сервере
    }
  }, [selectedFig, props]);

  const handleChangeProp = useCallback(
    (updatedProps: {
      radius?: number;
      zoom_level?: string;
    }) => {
      if (selectedFig) {
        const updatedFig: ICommonFig = {
          ...selectedFig,
          ...updatedProps, // Merge updated properties into selectedFig
        };
        appDispatch(MarkersStore.selectMarkerLocally(updatedFig));
      }
    },
    [selectedFig]
  );
  const handleChangeGeo = useCallback(
    (updatedProps: {
      geometry?: IPointCoord | IPolygonCoord | IPolylineCoord;
    }) => {
      if (selectedFig) {
        const updatedFig: ICommonFig = {
          ...selectedFig,
          ...updatedProps, // Merge updated properties into selectedFig
        };
        appDispatch(MarkersStore.selectMarkerLocally(updatedFig));
      }
    },
    [selectedFig]
  );

  useEffect(() => {
    if (props.events) {
      props.events.clickSave = handleSave;
    }
  }, [props.events, selectedFig]);

  const addDefaultGeometry = useCallback(() => {
    if (!objProps) return;

    const figure: ICircle = {
      ...objProps,
      geometry: { type: PointType, coord: [0, 0] },
      radius: 100,
    };

    appDispatch(MarkersStore.selectMarkerLocally(figure));
  }, [selected_id, objProps, appDispatch]);

  const changeGeometryType = useCallback(
    (newType: GeometryType) => {
      if (!selectedFig || !selectedFig.geometry) return;

      let newGeometry: IPointCoord | IPolygonCoord | IPolylineCoord;

      const oldGeom = selectedFig.geometry;

      switch (newType) {
        case PointType: {
          // Если текущая геометрия массив (Polygon/Polyline), берем первую точку
          let coord: LatLngPair = [0, 0];
          if (oldGeom.type === PolygonType || oldGeom.type === LineStringType) {
            const coordsArray = oldGeom.coord as LatLngPair[];
            if (coordsArray.length > 0) coord = coordsArray[0];
          } else if (oldGeom.type === PointType && oldGeom.coord) {
            coord = oldGeom.coord;
          }
          newGeometry = { type: PointType, coord };
          break;
        }
        case PolygonType: {
          // Если текущая геометрия Point, берем coord и делаем массив
          let coords: LatLngPair[] = [];
          if (oldGeom.type === PointType && oldGeom.coord) {
            coords = [oldGeom.coord];
          } else if ((oldGeom.type === PolygonType || oldGeom.type === LineStringType) && Array.isArray(oldGeom.coord)) {
            coords = oldGeom.coord;
          }
          newGeometry = { type: PolygonType, coord: coords };
          break;
        }
        case LineStringType: {
          let coords: LatLngPair[] = [];
          if (oldGeom.type === PointType && oldGeom.coord) {
            coords = [oldGeom.coord];
          } else if ((oldGeom.type === PolygonType || oldGeom.type === LineStringType) && Array.isArray(oldGeom.coord)) {
            coords = oldGeom.coord;
          }
          newGeometry = { type: LineStringType, coord: coords };
          break;
        }
      }

      const updatedFig: ICommonFig = {
        ...selectedFig,
        geometry: newGeometry,
      };

      appDispatch(MarkersStore.selectMarkerLocally(updatedFig));
    },
    [selectedFig, appDispatch]
  );



  return (
    <Box style={{ width: '100%' }}>

      {(!selectedFig?.geometry && selected_id) && (
        <Button
          variant="contained"
          size="small"
          onClick={addDefaultGeometry}
         >
          Add Geo
        </Button>
      )}
      {selectedFig ? (
        <>
        <GeoEditor
          props={{
            val: selectedFig.geometry,  // Передача геометрии в GeoEditor
            prop_name: 'geometry',
            handleChangeProp: (event: any) => {
              handleChangeGeo(event);
            },
          }}
        />
        <GeoExtraPropertiesEditor
            extraProps={{
              radius: selectedFig.radius,
              zoom_level: selectedFig.zoom_level,
            }}
            showRadius={selectedFig?.geometry?.type === PointType}
            handleChangeProp={(updatedProps) => handleChangeProp(updatedProps)}
          />
          <FormControl size="small" sx={{ mb: 1, minWidth: 120 }}>
            <InputLabel id="geometry-type-label">Тип гео</InputLabel>
            <Select
              labelId="geometry-type-label"
              value={
                selectedFig.geometry?.type === PointType
                  ? "Point"
                  : selectedFig.geometry?.type === "Polygon"
                    ? "Polygon"
                    : "LineString"
              }
              label="Тип гео"
              onChange={(e) => changeGeometryType(e.target.value as "Point" | "Polygon" | "LineString")}
            >
              <MenuItem value="Point">Point</MenuItem>
              <MenuItem value="Polygon">Polygon</MenuItem>
              <MenuItem value="LineString">LineString</MenuItem>
            </Select>
          </FormControl>
          </>
      ) : null
      }
    </Box>
  );
};

export default SelectedObjectGeoEditor;
