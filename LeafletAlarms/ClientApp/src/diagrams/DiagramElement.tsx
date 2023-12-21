import * as React from 'react';

import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';

import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import { IDiagramDTO, IDiagramTypeDTO } from '../store/Marker';

interface IDiagramElement {
  diagram: IDiagramDTO;
  zoom: number;
}

export default function DiagramElement(props: IDiagramElement) {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagrams = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);
  const diagram = props.diagram

  const appDispatch = useAppDispatch();
  const selectItem = useCallback(
    (diagram_id: string) => {
      appDispatch<any>(GuiStore.actionCreators.selectTreeItem(diagram_id));
    }, [objProps]);


  if (diagram == null) {
    return null;
  }

  var diagram_type: IDiagramTypeDTO = null;

  if (diagrams.dgr_types != null) {
    diagram_type = diagrams.dgr_types.find(t => t.name == diagram.dgr_type);
  }
  var zoom = props.zoom;

  return (
    <React.Fragment>
      <Box onClick={() => { selectItem(diagram?.id) }}
        sx={{// Main object
          border: 0,
          padding: 0,
          margin: 0,
          position: 'absolute',

          top: diagram.geometry.top * zoom + 'px', // Сдвиг от верхнего края
          left: diagram.geometry.left * zoom + 'px', // Сдвиг от левого края
          height: diagram.geometry.height * zoom + 'px',
          width: diagram.geometry.width * zoom + 'px',
          backgroundColor: 'transparent',
          '&:hover': {
            cursor: 'pointer'
          },
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
          }} />

      </Box>

    </React.Fragment>
  );
}