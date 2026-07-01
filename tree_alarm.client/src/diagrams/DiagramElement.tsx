/* eslint-disable no-undef */
import * as React from 'react';

import { ApplicationState } from '../store/index';
import { useSelector } from 'react-redux';

import { useAppDispatch } from '../store/configureStore';
import { useCallback, useEffect, useRef, useState } from 'react';
import { Box } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import * as ValuesStore from '../store/ValuesStates';
import * as DiagramsStore from '../store/DiagramsStates';
import { DeepCopy, IDiagramCoord, IDiagramDTO, IDiagramTypeDTO, IDiagramTypeRegionDTO, IValueDTO, TreeMarker } from '../store/Marker';
import { DiagramRotationHandle } from './DiagramRotationHandle';
import { useDiagramEditing } from '../editworkspace/DiagramEditingContext';


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
  const region_value = cur_values.find(v => v.name === region.region_key);

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

  const defaultStyles = {
    color: region.styles?.color || 'black',
    fontSize: region.styles?.fontSize || '14px',
    backgroundColor: region.styles?.backgroundColor || 'transparent',
  };

  // Создаем компонент, который будет отображать значение региона
  return (
    <Box
      key={"box for region " + region_value.id}
      sx={{
        padding: 0,
        margin: 0,
        position: 'absolute',
        top: coord.top * zoom + 'px',
        left: coord.left * zoom + 'px',
        height: coord.height * zoom + 'px',
        width: coord.width * zoom + 'px',
        ...defaultStyles,
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'left',
        alignItems: 'left',
        overflow: 'hidden',
      }}
    >
      {
        `${region_value.name}: ${region_value.value}`
      }      
      
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

  const valuesMap: Record<string, IValueDTO[]>|null = useSelector((state: ApplicationState) => state?.valuesStates?.valuesMap) ?? null;
  

  const { diagram, parent, getColor, parent_coord, zoom } = props;

  const cur_values: IValueDTO[] = useSelector(ValuesStore.selectValuesMapForOwner(diagram.id)) ?? [];
 

  const children: TreeMarker[] = diagram_content?.children?.filter(i => i.parent_id === diagram?.id) || [];
  const childrenDiagrams = diagram_content?.content?.filter(e => children.some(c => c.id === e.id)) || null;

  const appDispatch = useAppDispatch();

  const [coord, setCoord] = useState(diagram.geometry);

  // Lets a freely-placed object (no region_id — region-bound children get their position
  // from the parent type's region layout instead, see the effect below) be repositioned by
  // dragging it directly, rather than only via the numeric left/top fields in Properties.
  // Tracks the drag via window-level listeners (same pattern as DiagramRotationHandle /
  // CompassDial) instead of setPointerCapture — capture only engages reliably on the exact
  // node it's requested on, and a fast drag easily outruns this object's small hit area;
  // window listeners keep firing regardless of where the cursor physically is.
  const editingEnabled = useDiagramEditing();
  const justDraggedRef = useRef(false);
  const canDrag = editingEnabled && diagram?.region_id == null;

  const handlePointerDown = (event: React.PointerEvent<HTMLDivElement>) => {
    if (!canDrag || event.button !== 0) return;
    // Stop a child diagram's drag gesture from also being picked up as a drag of this
    // (ancestor) element.
    event.stopPropagation();

    const startX = event.clientX;
    const startY = event.clientY;
    const startLeft = coord.left;
    const startTop = coord.top;
    let dragging = false;

    const onMove = (ev: PointerEvent) => {
      const dx = (ev.clientX - startX) / zoom;
      const dy = (ev.clientY - startY) / zoom;

      if (!dragging && Math.hypot(dx, dy) < 3) return;
      dragging = true;

      const updated = DeepCopy(diagram);
      if (!updated) return;
      updated.geometry = {
        ...updated.geometry,
        left: startLeft + dx,
        top: startTop + dy,
      };
      appDispatch(DiagramsStore.update_diagram_geometry_locally(updated));
    };

    const onUp = () => {
      window.removeEventListener('pointermove', onMove);
      window.removeEventListener('pointerup', onUp);
      if (dragging) {
        justDraggedRef.current = true;
      }
    };

    window.addEventListener('pointermove', onMove);
    window.addEventListener('pointerup', onUp);
  };

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    event.stopPropagation();
    if (justDraggedRef.current) {
      justDraggedRef.current = false;
      return;
    }
    selectItem(diagram?.id);
  };

  const selectItem = useCallback(
    (diagram_id: string) => {
      appDispatch(GuiStore.selectTreeItem(diagram_id));
      console.log("selecting diagram:", diagram_id);
    }, [appDispatch]);

  useEffect(
    () => {
      console.log(valuesMap);
    }, [valuesMap]);
  useEffect(
    () => {
      var newCoord = diagram.geometry;

      if (diagram?.region_id != null) {
        var parent_type: IDiagramTypeDTO|null = diagram_content?.dgr_types?.find(t => t.name == parent?.dgr_type) ?? null;

        if (parent_type != null) {
          var region = parent_type.regions?.find(r => r.region_key == diagram.region_id);

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
        onPointerDown={handlePointerDown}
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
          touchAction: canDrag ? 'none' : undefined,
          '&:hover': {
            cursor: canDrag ? 'move' : 'pointer'
          },
          display: 'flex', // Добавляем свойство display и flex-direction
          flexDirection: 'column',
        }}
      >


        <img
          key={"img" + diagram?.id}
          src={diagram_type?.src != null ? diagram_type?.src : "svg/black_square.svg"}
          draggable={false}
          style={{
            border: 0,
            padding: 0,
            margin: 0,
            width: '100%',
            height: '100%',
            objectFit: 'fill',
            transform: `rotate(${coord.rotation ?? 0}deg)`
          }} />

        {editingEnabled && selected_id === diagram?.id && (
          <DiagramRotationHandle
            onRotate={(deg) => {
              const updated = DeepCopy(diagram);
              if (!updated) return;
              updated.geometry = { ...updated.geometry, rotation: deg };
              appDispatch(DiagramsStore.update_diagram_geometry_locally(updated));
            }}
          />
        )}

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