import * as React from 'react';

import Button from '@mui/material/Button';
import { Box, Menu, MenuItem} from '@mui/material';
import * as EditStore from '../store/EditStates';
import { useDispatch, useSelector } from 'react-redux';
import { IObjProps, LineStringType, PointType, PolygonType } from '../store/Marker';
import * as ObjPropsStore from '../store/ObjPropsStates';

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
        geometry: "{\"coord\":[]}",
        type: PolygonType,
        extra_props: null
      };
      dispatch(EditStore.actionCreators.setFigureEditMode(true));
      dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
      return;
    }
   
    if (item == EditStore.PolylineTool) {
      const copy: IObjProps = {
        id: null,
        name: 'New Polyline 1',
        parent_id: selected_id,
        geometry: "{\"coord\":[]}",
        type: LineStringType,
        extra_props: null
      };
      dispatch(EditStore.actionCreators.setFigureEditMode(true));
      dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
      return;
    }

    if (item == EditStore.CircleTool) {
      const copy: IObjProps = {
        id: null,
        name: 'New Circle 1',
        parent_id: selected_id,
        geometry: null,
        type: PointType,
        extra_props: null
      };
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
        <Button onClick={handleClickListItem} style={{ textTransform: 'none' }}>
          New object
        </Button>
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