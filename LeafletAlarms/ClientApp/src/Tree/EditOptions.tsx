import * as React from 'react';

import Button from '@mui/material/Button';
import { Box, IconButton, Menu, MenuItem} from '@mui/material';
import * as EditStore from '../store/EditStates';
import { useDispatch, useSelector } from 'react-redux';
import { IObjProps, IPointCoord, IPolygonCoord, IPolylineCoord, LineStringType, PointType, PolygonType, setExtraProp } from '../store/Marker';
import * as ObjPropsStore from '../store/ObjPropsStates';
import AddIcon from '@mui/icons-material/Add';

export default function EditOptions() {

  const dispatch = useDispatch();
  
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const open = Boolean(anchorEl);
  const selected_id = useSelector((state) => state?.guiStates?.selected_id);

  const handleClickListItem = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuItemClick = (
    item: string,
    index: number,
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
      dispatch(EditStore.actionCreators.setFigureEditMode(true));
      dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
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
      dispatch(EditStore.actionCreators.setFigureEditMode(true));
      dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
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
      dispatch(EditStore.actionCreators.setFigureEditMode(true));
      dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
      return;
    }
    dispatch(EditStore.actionCreators.setFigureEditMode(false));
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  return (
    <Box
      key="Edit_Options"
      sx={{
      width: '100%',
      bgcolor: 'background.paper',

      height: 'auto',
      border: 1,
      
    }}>
      <IconButton onClick={handleClickListItem} size="small"  style={{ textTransform: 'none' }}>
        
        <AddIcon fontSize="inherit" />
        <Box>New object</Box>
        
      </IconButton>
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

            {Object.entries(EditStore.Figures).map((item, index) => (
                  <MenuItem
                    key={item[0]}
                onClick={(event) => handleMenuItemClick(item[0], index)}>
                {item[1]}
                  </MenuItem>
                ))}
                
            </Menu>
    </Box>
  );
}