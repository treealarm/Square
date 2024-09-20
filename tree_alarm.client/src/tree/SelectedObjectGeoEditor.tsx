/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import React, { useCallback, useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import { ICommonFig, IFigures } from '../store/Marker';
import GeoEditor from '../prop_controls/geo_editor';
import { useAppDispatch } from '../store/configureStore';
import * as MarkersStore from '../store/MarkersStates';
import { Box } from '@mui/material';

export interface ChildEvents {
  clickSave: () => void;
}

// ��������� ��� props ��������� ����������
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
            const fetchedFigures = await MarkersStore.requestMarkersByIds([selected_id]); // ���� ��������� �������
            appDispatch(MarkersStore.selectMarkerLocally(fetchedFigures.figs[0])); // ������������� ������ ������
          } catch (error) {
            console.error("Failed to fetch marker:", error);
            appDispatch(MarkersStore.selectMarkerLocally(null)); // ������������� null � ������ ������
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
        figs: [selectedFig],  // ����� ������ figs
      };
      appDispatch(MarkersStore.updateMarkers(figures));  // ��������� ������ �� �������
    }
  }, [selectedFig, props]);

  const handleChangeProp = useCallback((event: any) => {
    if (selectedFig) {
      const fig: ICommonFig = {
        ...selectedFig, // ��������� ��� �������� �� selectedFig
        geometry: JSON.parse(event.target.value), // �������� ������ geometry
      };
      appDispatch(MarkersStore.selectMarkerLocally(fig));
    }
  }, [selectedFig]);  // ��������� �����������


  useEffect(() => {
    if (props.events) {
      props.events.clickSave = handleSave;
    }
  }, [props.events, selectedFig]);



  return (
    <Box style={{ width: '100%' }}>
      {selectedFig ? (
        <GeoEditor
          props={{
            val: selectedFig.geometry,  // �������� ��������� � GeoEditor
            prop_name: 'geometry',
            handleChangeProp: (event: any) => {
              handleChangeProp(event);
            },
          }}
        />
      ) : null
      }
    </Box>
  );
};

export default SelectedObjectGeoEditor;
