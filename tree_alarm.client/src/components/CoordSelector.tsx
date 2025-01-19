/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import { useState, useEffect, MouseEvent } from 'react';
import { MapContainer, TileLayer, Marker, useMapEvents, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { Popover, Button, IconButton, Box } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import { MyCommonFig } from '../map/MyCommonFig';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import { DeepCopy, ICommonFig, LatLngPair, LineStringType, PointType, PolygonType } from '../store/Marker';

var url = 'http://';

if (window.location.protocol == "https:") {
  url = 'https://';
}

url = url
  + window.location.hostname
  + ':'
  + window.location.port
  + '/api/Map/GetTiles'
  + '/layer/{z}/{x}/{y}.png';

interface CoordSelectorProps {
  lat: number | null;
  lon: number | null;
  index: number;
  onConfirm: (lat: number, lon: number) => void;
}

export function CoordSelector({ lat, lon, index, onConfirm }: CoordSelectorProps) {
  const fallbackPosition: [number, number] = [55.751137, 37.600408]; // Fallback if coordinates are invalid
  const [position, setPosition] = useState<[number, number]>(fallbackPosition);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

  const selectedFig = useSelector((state: ApplicationState) => state?.markersStates?.selected_marker);

  const [curFig, setCurFig] = useState<ICommonFig | null>(selectedFig ?? null);

  function OnClick(newPos: LatLngPair) {
    setPosition(newPos); // Сохраняем новые координаты

    let updatedCoord: LatLngPair | LatLngPair[] | null;    

    // Проверяем тип геометрии и обновляем `coord` в зависимости от этого
    if (curFig?.geometry?.type === PointType) {
      updatedCoord = newPos; // Для Point — одна пара координат

    } else if (curFig?.geometry?.type === PolygonType || curFig?.geometry?.type === LineStringType) {
      updatedCoord = [...(curFig.geometry.coord as LatLngPair[])]; // Для Polygon и LineString — массив координат
      if (curFig.geometry.coord.length > index) {
        updatedCoord[index] = newPos; // Обновляем по индексу
      }
    } else {
      updatedCoord = null; // Если тип неизвестен
    }

    if (curFig) {
      var updatedFig: ICommonFig | null = DeepCopy(curFig);
      if (updatedFig) {
        updatedFig.geometry.coord = updatedCoord;
        setCurFig(updatedFig); // Сохраняем обновлённую фигуру в состоянии
      }      
    }
    
  }

  useEffect(() => {
    if (lat !== null && lon !== null) {
      setPosition([lat, lon]);
    } else {
      // Получаем текущее местоположение как запасной вариант
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          setPosition([pos.coords.latitude, pos.coords.longitude]);
        },
        () => {
          setPosition(fallbackPosition);
        }
      );
    }
  }, [lat, lon]);


  useEffect(() => {
    setCurFig(selectedFig??null);
  }, [selectedFig]);

  const handleClick = (event: MouseEvent<HTMLElement>) => {
    if (lat !== null && lon !== null) {
      setPosition([lat, lon]);
    }
    setAnchorEl(event.currentTarget);
    setCurFig(selectedFig ?? null);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const open = Boolean(anchorEl);
  const id = open ? 'coord-selector-popover' : undefined;

  // Component to handle map clicks
  const LocationMarker = () => {
  useMapEvents({
      click(e:any) {
      const newPos: LatLngPair = [e.latlng.lat, e.latlng.lng];
      OnClick(newPos);
    },
  });
    const parentMap = useMap();
    parentMap.attributionControl.options.prefix =
      '<a href="https://www.leftfront.org" title="A JavaScript library for interactive maps">' + '<img width="12" height="8" src="https://upload.wikimedia.org/wikipedia/commons/a/a9/Flag_of_the_Soviet_Union.svg"></img>' + 'LeafletAlarms</a>';
    return <Marker position={position} />;
  };

  return (
    <div>
      <IconButton
        edge="end"
        color="primary"
        onClick={handleClick}
      >
        <EditIcon />
      </IconButton>
      <Popover
        id={id}
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'center',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'center',
        }}
      >
        <div style={{ padding: 10 }}>
          <MapContainer center={position} zoom={13} style={{ height: "400px", width: "400px" }}>
            <TileLayer
            maxZoom={20}
            attribution="&copy; <a href=&quot;https://www.openstreetmap.org/copyright&quot;>OpenStreetMap</a> contributors"
            url={url}
            key={1}
          />
            <LocationMarker />
            {
              curFig ? (
                <MyCommonFig
                  key={curFig?.id}
                  marker={curFig}
                  hidden={false}
                  pathOptions={{}}
                />
              ) : null
            }
          </MapContainer>

          <div style={{ marginTop: 10 }}>
            <Button
              variant="contained"
              color="primary"
              onClick={() => {
                onConfirm(position[0], position[1]); // Return lat and lon
                handleClose();
              }}
            >
              OK
            </Button>
            <Button variant="outlined"
              onClick={() => {
                handleClose();
                }
              } style={{ marginLeft: 10 }}>
              Cancel
            </Button>

            {/* Добавление отображения текущих координат рядом с кнопкой */}
            <Box ml={2}>
              <span>Lat: {position[0]}, Lon: {position[1]}</span>
            </Box>

          </div>
        </div>
      </Popover>
    </div>
  );
}
