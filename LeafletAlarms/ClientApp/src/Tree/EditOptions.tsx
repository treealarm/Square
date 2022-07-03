import * as React from 'react';

import Button from '@mui/material/Button';
import { Menu, MenuItem} from '@mui/material';
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
        name: 'New Polygon',
        parent_id: selected_id,
        geometry: "[]",
        type: PolygonType,
        extra_props: null
      };
      dispatch(EditStore.actionCreators.setFigureEditMode(true));
      dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
      return;
    }

    if (item == EditStore.PolylineTool) {
      const copy: IObjProps = {
        name: 'New Polyline',
        parent_id: selected_id,
        geometry: "[]",
        type: LineStringType,
        extra_props: null
      };
      dispatch(EditStore.actionCreators.setFigureEditMode(true));
      dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
      return;
    }

    if (item == EditStore.CircleTool) {
      const copy: IObjProps = {
        name: 'New Circle',
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
    <div>
      
      <React.Fragment key={"Edit_Options"}>
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
        </React.Fragment>
    </div>
  );
}