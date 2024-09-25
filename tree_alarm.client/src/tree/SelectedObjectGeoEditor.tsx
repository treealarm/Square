/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import React, { useCallback, useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import { ICommonFig, IFigures, PointType } from '../store/Marker';
import GeoEditor from '../prop_controls/geo_editor';
import { useAppDispatch } from '../store/configureStore';
import * as MarkersStore from '../store/MarkersStates';
import { Box } from '@mui/material';
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
    (updatedProps: { geometry?: any; radius?: number; zoom_level?: string; }) => {
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



  return (
    <Box style={{ width: '100%' }}>
      {selectedFig ? (
        <>
        <GeoEditor
          props={{
            val: selectedFig.geometry,  // Передача геометрии в GeoEditor
            prop_name: 'geometry',
            handleChangeProp: (event: any) => {
              handleChangeProp(event);
            },
          }}
        />
        <GeoExtraPropertiesEditor
            extraProps={{
              radius: selectedFig.radius,
              zoom_level: selectedFig.zoom_level,
            }}
            showRadius={selectedFig.geometry.type === PointType}
            handleChangeProp={(updatedProps) => handleChangeProp(updatedProps)}
          />
          </>
      ) : null
      }
    </Box>
  );
};

export default SelectedObjectGeoEditor;
