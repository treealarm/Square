import * as React from 'react';

import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';

import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box} from '@mui/material';
import * as GuiStore from '../store/GUIStates';

interface IDiagramElement {
  parent_id: string;
  zoom: number;
}

export default function DiagramElement(props: IDiagramElement) {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  const appDispatch = useAppDispatch();
  const selectItem = useCallback(
    (diagram_id: string) => {
      //appDispatch<any>(DiagramsStore.reset_diagram(null));
      appDispatch<any>(GuiStore.actionCreators.selectTreeItem(diagram_id));
    }, [objProps]);


  if (diagram == null) {
    return null;
  }

  var content = diagram.content.filter(e => e.parent_id == props.parent_id);
  var zoom = props.zoom;

  return (
    <React.Fragment>

          {
            content.map((diagram, index) =>
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

                  <img src="svg/rack.svg"
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
            )}

      </React.Fragment>
  );
}