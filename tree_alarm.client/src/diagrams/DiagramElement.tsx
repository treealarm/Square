import * as React from 'react';

import { ApplicationState } from '../store/index';
import { useSelector } from 'react-redux';

import { useAppDispatch } from '../store/configureStore';
import { useCallback, useEffect, useState } from 'react';
import { Box, Typography } from '@mui/material';
import * as GuiStore from '../store/GUIStates';

import { IDiagramCoord, IDiagramDTO, IDiagramTypeDTO, IDiagramTypeRegionDTO, IValueDTO, TreeMarker } from '../store/Marker';


const getValueByRegion = (
  region: IDiagramTypeRegionDTO,
  cur_values: IValueDTO[],
  zoom: number,
  parent_coord: IDiagramCoord
) => {
  if (!region || !cur_values) {
    return null;
  }

  // Находим значение, соответствующее региону
  const region_value = cur_values.find(v => v.name === region.id);

  if (!region_value) {
    return null;
  }

  var coord = region?.geometry;

  if (region?.geometry != null) {
    var w = parent_coord.width;
    var h = parent_coord.height;
    coord = {
      top: h * region.geometry.top,
      left: w * region.geometry.left,
      height: h * region.geometry.height,
      width: w * region.geometry.width
    };
  }

  // Создаем компонент, который будет отображать значение региона
  return (
    <Box
      key={"box for region " + region_value.id}
      sx={{
        padding: 0,
        margin: 0,
        position: 'absolute',
        top: coord.top * zoom + 'px', // Сдвиг от верхнего края
        left: coord.left * zoom + 'px', // Сдвиг от левого края
        height: coord.height * zoom + 'px',
        width: coord.width * zoom + 'px',
        backgroundColor: 'green',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'left',
        alignItems: 'left',
        overflow: 'hidden', // Обрезаем текст, если он выходит за пределы
      }}
    >
      <Typography
        variant="body2"
        component="span"
        color="black"
        sx={{
          whiteSpace: 'nowrap', // Запрещаем перенос строк
          overflow: 'hidden',   // Обрезаем переполненный текст
          textOverflow: 'ellipsis', // Добавляем многоточие, если текст не помещается
        }}
      >
        {`${region_value.name}: ${region_value.value}`}
      </Typography>
    </Box>
  );
};


interface IDiagramElement {
  diagram: IDiagramDTO;
  parent: IDiagramDTO|null;
  parent_coord: IDiagramCoord
  zoom: number;
  z_index: number;
  // eslint-disable-next-line no-unused-vars
  getColor: (marker: IDiagramDTO) => any;
}

export default function DiagramElement(props: IDiagramElement) {

  const diagram_content = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content);
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const values = useSelector((state: ApplicationState) => state?.valuesStates?.values);


  const { diagram, parent, getColor, parent_coord, zoom } = props;

  const cur_values: IValueDTO[] = values?.filter(v => v.owner_id == diagram?.id) ?? [];

  const children: TreeMarker[] = diagram_content?.children?.filter(i => i.parent_id === diagram?.id) || [];
  const childrenDiagrams = diagram_content?.content?.filter(e => children.some(c => c.id === e.id)) || null;

  const appDispatch = useAppDispatch();

  const [coord, setCoord] = useState(diagram.geometry);


  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    event.stopPropagation();
    selectItem(diagram?.id);
  };

  const selectItem = useCallback(
    (diagram_id: string) => {
      appDispatch(GuiStore.selectTreeItem(diagram_id));
      console.log("selecting diagram:", diagram_id);
    }, [appDispatch]);

  useEffect(
    () => {
      var newCoord = diagram.geometry;

      if (diagram?.region_id != null) {
        var parent_type: IDiagramTypeDTO|null = diagram_content?.dgr_types.find(t => t.name == parent?.dgr_type) ?? null;

        if (parent_type != null) {
          var region = parent_type.regions.find(r => r.id == diagram.region_id);

          if (region != null) {
            var w = parent_coord.width;
            var h = parent_coord.height;
            newCoord = 
              {
                top: h * region.geometry.top,
                left: w * region.geometry.left,
                height: h * region.geometry.height,
                width: w * region.geometry.width
              };
          }
        }
      }

      if (newCoord == null) {
        newCoord = 
          {
            top: diagram.geometry.top,
            left: diagram.geometry.left,
            height: diagram.geometry.height,
            width: diagram.geometry.width
          };
      }
      setCoord(newCoord);
    }, [diagram, diagram_content, parent?.dgr_type, parent_coord.height, parent_coord.width]);


  var diagram_type: IDiagramTypeDTO|null = null;

  if (diagram_content?.dgr_types != null) {
    diagram_type = diagram_content?.dgr_types.find(t => t.name == diagram?.dgr_type) ?? null;
  }

  if (diagram_type == null) {
    console.log("no type");
  }

  var color = getColor(diagram);
  var shadow = null;

  if (selected_id == diagram?.id) {
    shadow = '0 0 15px 0px rgba(0,0,0,0.9)';
  }

  if (diagram == null) {
    return null;
  }

  return (
    <React.Fragment>

      <Box
        key={"box in element"}
        onClick={handleClick}
        sx={{// Main object
          boxShadow: shadow,
          padding: 0,
          margin: 0,
          position: 'absolute',
          top: coord.top * zoom + 'px', // Сдвиг от верхнего края
          left: coord.left * zoom + 'px', // Сдвиг от левого края
          height: coord.height * zoom + 'px',
          width: coord.width * zoom + 'px',
          backgroundColor: 'transparent',
          color:color,
          '&:hover': {
            cursor: 'pointer'
          },
          display: 'flex', // Добавляем свойство display и flex-direction
          flexDirection: 'column',
        }}
      >


        <img
          key={"img" + diagram?.id}
          src={diagram_type?.src != null ? diagram_type?.src : "svg/black_square.svg"}
          style={{
            border: 0,
            padding: 0,
            margin: 0,
            width: '100%',
            height: '100%',
            objectFit: 'fill'
          }} />

        {diagram_type?.regions?.map((region) => getValueByRegion(region, cur_values, zoom, coord))}

        {
          childrenDiagrams?.map((dgr) =>
            <DiagramElement
              diagram={dgr}
              parent={diagram}
              parent_coord={ coord }
              zoom={zoom}
              z_index={props.z_index + 1}
              getColor={props.getColor}
              key={dgr?.id} />
          )
        }

        {
          color == null ? null :
            <svg width="100%" height="100%" xmlns="http://www.w3.org/2000/svg"
              pointerEvents='none'
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
              }}>

              <rect x="0" y="0" width="100%" height="100%"
                fill={color} fillOpacity="0.1"
                stroke={color} strokeWidth="5" opacity="0.5" />
            </svg>
        }
        
      </Box>      
      
    </React.Fragment>
  );
}