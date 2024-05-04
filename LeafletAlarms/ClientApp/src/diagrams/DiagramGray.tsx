import * as React from 'react';

import { ApplicationState } from '../store/index';
import { useSelector } from 'react-redux';

import { Box } from '@mui/material';
import { IDiagramCoord, IDiagramDTO, IDiagramTypeDTO } from '../store/Marker';

interface IDiagramElement {
  diagram: IDiagramDTO;
  zoom: number;
}

export default function DiagramGray(props: IDiagramElement) {

  const diagrams = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  const diagram = props.diagram

  if (diagram == null) {
    return null;
  }

  var diagram_type: IDiagramTypeDTO = null;

  if (diagrams.dgr_types != null) {
    diagram_type = diagrams.dgr_types.find(t => t.name == diagram.dgr_type);
  }

  if (diagram_type == null) {
    return null;
  }
  var zoom = props.zoom;

  var coord: IDiagramCoord = null;

  if (diagram.geometry != null && diagram.geometry.height != 0 && diagram.geometry.width != 0) {
    coord =
    {
      top: 0,
      left: 0,
      height: diagram.geometry.height,
      width: diagram.geometry.width
    };
  }





  return (
    <React.Fragment>

      {!coord ? (
        <img
          src={diagram_type?.src}
          style={{
            border: 0,
            padding: 0,
            margin: 0,
            filter: 'grayscale(100%)', // Применение эффекта чёрно-белого изображения
            opacity: 0.5, // Устанавливаем полупрозрачность (значение от 0 до 1)
          }}
        />
      ) : (

        <Box
          sx={{
            padding: 0,
            margin: 0,
            position: 'absolute',

            top: coord.top * zoom + 'px', // Сдвиг от верхнего края
            left: coord.left * zoom + 'px', // Сдвиг от левого края
            height: coord.height * zoom + 'px',
            width: coord.width * zoom + 'px',
            backgroundColor: 'transparent',
            display: 'flex', // Добавляем свойство display и flex-direction
            flexDirection: 'column',
          }}
        >

          <img src={diagram_type?.src}
            style={{
              border: 0,
              padding: 0,
              margin: 0,
              width: '100%',
              height: '100%',
              objectFit: 'fill', // Заполняет SVG без сохранения пропорций
              filter: 'grayscale(100%)', // Применение эффекта чёрно-белого изображения
              opacity: 0.5, // Устанавливаем полупрозрачность (значение от 0 до 1)

            }} />

        </Box>
      )}
    </React.Fragment>
  );
}