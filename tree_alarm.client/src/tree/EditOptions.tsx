import * as React from 'react';

import { Box, IconButton, Menu, MenuItem, Tooltip } from '@mui/material';
import * as EditStore from '../store/EditStates';
import * as DiagramsStore from '../store/DiagramsStates';
import { useSelector } from 'react-redux';
import { useAppDispatch } from '../store/configureStore';
import { IDiagramDTO, IObjProps, IPointCoord, IPolygonCoord, IPolylineCoord, LineStringType, PointType, PolygonType, setExtraProp } from '../store/Marker';
import * as ObjPropsStore from '../store/ObjPropsStates';
import AddIcon from '@mui/icons-material/Add';
import { ApplicationState } from '../store';

export default function EditOptions() {

  const appDispatch = useAppDispatch();

  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const open = Boolean(anchorEl);
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  var figuresMenu = EditStore.Figures;

  if (diagram != null) {
    figuresMenu = EditStore.Diagrams;
  }
  const handleClickListItem = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuItemClick = (
    item: string
  ) => {
    setAnchorEl(null);

    if (item == EditStore.PolygonTool) {
      const copy: IObjProps = {
        id: null,
        name: 'New Polygon 1',
        parent_id: selected_id,
        extra_props: null
      };

      var geometry1: IPolygonCoord =
      {
        coord: [],
        type: PolygonType
      }
      setExtraProp(copy, "geometry", JSON.stringify(geometry1), null);
      appDispatch(EditStore.setEditMode(true));
      appDispatch(ObjPropsStore.setObjPropsLocally(copy));
      return;
    }

    if (item == EditStore.PolylineTool) {
      const copy: IObjProps = {
        id: null,
        name: 'New Polyline 1',
        parent_id: selected_id,
        extra_props: null
      };
      var geometry2: IPolylineCoord =
      {
        coord: [],
        type: LineStringType
      }
      setExtraProp(copy, "geometry", JSON.stringify(geometry2), null);
      appDispatch(EditStore.setEditMode(true));
      appDispatch(ObjPropsStore.setObjPropsLocally(copy));
      return;
    }

    if (item == EditStore.CircleTool) {
      const copy: IObjProps = {
        id: null,
        name: 'New Circle 1',
        parent_id: selected_id,
        extra_props: null,
      };
      var geometry3: IPointCoord =
      {
        coord: null,
        type: PointType
      }
      setExtraProp(copy, "geometry", JSON.stringify(geometry3), null);
      appDispatch(EditStore.setEditMode(true));
      appDispatch(ObjPropsStore.setObjPropsLocally(copy));
      return;
    }

    if (item == EditStore.DiagramTool) {
      const copy: IDiagramDTO = {
        id: null,
        name: 'New Diagram',
        parent_id: selected_id,
        dgr_type: null,
        background_img:null,
        geometry:
        {
          left: 0,
          top: 0,
          width: 100,
          height:100
        },
        region_id:null
      };

      appDispatch(DiagramsStore.updateDiagrams([copy]));
      appDispatch(DiagramsStore.fetchDiagram(selected_id));
      return;
    }

    appDispatch(EditStore.setEditMode(false));
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  return (
    <Box>
      <Tooltip title={"Create new object"}>
        <IconButton onClick={handleClickListItem} style={{ textTransform: 'none' }}>

          <AddIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>
      <Menu
        id="lock-menu"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        MenuListProps={{
          'aria-labelledby': 'lock-button',
          role: 'listbox',
        }}
      >

        {Object.entries(figuresMenu).map((item) => (
          <MenuItem
            key={item[0]}
            onClick={() => handleMenuItemClick(item[0])}>
            {item[1]}
          </MenuItem>
        ))}

      </Menu>
    </Box>
  );
}