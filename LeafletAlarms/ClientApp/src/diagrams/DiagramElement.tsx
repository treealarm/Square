import * as React from 'react';

import { ApplicationState } from '../store/index';
import { useSelector } from 'react-redux';

import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import { IDiagramCoord, IDiagramDTO, IDiagramTypeDTO } from '../store/Marker';

interface IDiagramElement {
  diagram: IDiagramDTO;
  parent: IDiagramDTO;
  parent_coord: IDiagramCoord
  zoom: number;
  z_index: number;
  getColor: (marker: IDiagramDTO) => any;
}

export default function DiagramElement(props: IDiagramElement) {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagrams = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const diagram = props.diagram
  const parent = props.parent
  const getColor = props.getColor
  const parent_coord = props.parent_coord;

  const appDispatch = useAppDispatch();

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    e.stopPropagation();
    selectItem(diagram?.id);
  };

  const selectItem = useCallback(
    (diagram_id: string) => {
      appDispatch<any>(GuiStore.actionCreators.selectTreeItem(diagram_id));
      console.log("selecting diagram:", diagram_id);
    }, [objProps]);


  if (diagram == null) {
    return null;
  }

  var diagram_type: IDiagramTypeDTO = null;

  if (diagrams.dgr_types != null) {
    diagram_type = diagrams.dgr_types.find(t => t.name == diagram.dgr_type);
  }
  var zoom = props.zoom;
  var content = diagrams.content.filter(e => e.parent_id == diagram.id);

  var coord: IDiagramCoord = diagram.geometry;

  if (diagram.region_id != null) {
    var parent_type: IDiagramTypeDTO = diagrams.dgr_types.find(t => t.name == parent.dgr_type);

    if (parent_type != null) {
      var region = parent_type.regions.find(r => r.id == diagram.region_id);

      if (region != null) {
        var w = parent_coord.width;
        var h = parent_coord.height;
        coord =
        {
          top: h * region.geometry.top,
          left: w * region.geometry.left,
          height: h * region.geometry.height,
          width: w * region.geometry.width
        };
      }
    }
  }

  if (coord == null) {
    coord =
    {
      top: diagram.geometry.top,
      left: diagram.geometry.left,
      height: diagram.geometry.height,
      width: diagram.geometry.width
    };
  }

  var color = getColor(diagram);
  var shadow = null;

  if (selected_id == diagram?.id) {
    shadow = '0 0 15px 0px rgba(0,0,0,0.9)';
  }

  return (
    <React.Fragment>

      <Box
        key={"box in element"}
        onClick={handleClick//() => { selectItem(diagram?.id) }
        }
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
          src={diagram_type?.src}
          style={{
            border: 0,
            padding: 0,
            margin: 0,
            width: '100%',
            height: '100%',
            objectFit: 'fill'
          }} />

        {
          content?.map((dgr, index) =>
            <DiagramElement
              diagram={dgr}
              parent={diagram}
              parent_coord={ coord }
              zoom={zoom}
              z_index={props.z_index + 1}
              getColor={props.getColor}
              key={dgr?.id} />
          )}
        {
          color == null ? null :
            <svg width="100%" height="100%" xmlns="http://www.w3.org/2000/svg"
              pointer-events='none'
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
              }}>

              <rect x="0" y="0" width="100%" height="100%"
                fill={color} fill-opacity="0.1"
                stroke={color} strokeWidth="5" opacity="0.5" />
            </svg>
        }
        
      </Box>      
      
    </React.Fragment>
  );
}