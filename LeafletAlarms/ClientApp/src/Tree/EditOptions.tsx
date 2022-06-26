import * as React from 'react';
import Drawer from '@mui/material/Drawer';
import Button from '@mui/material/Button';
import { FormControl, FormControlLabel, FormLabel, Menu, MenuItem, Radio, RadioGroup, SpeedDial, SpeedDialIcon } from '@mui/material';
import * as EditStore from '../store/EditStates';
import { useDispatch, useSelector } from 'react-redux';

export default function EditOptions() {

  const dispatch = useDispatch();
  const selectedTool = useSelector((state) => state.editState.figure);
  
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const open = Boolean(anchorEl);

  const handleClickListItem = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuItemClick = (
    item: string,
    index: number,
  ) => {
    setAnchorEl(null);
    dispatch(EditStore.actionCreators.setFigureEditMode(item, false));
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  return (
    <div>
      
      <React.Fragment key={"Edit_Options"}>
        <Button onClick={handleClickListItem} style={{ textTransform: 'none' }}>
          Current tool: {EditStore.Figures[selectedTool]}
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
                selected={selectedTool === item[0]}
                onClick={(event) => handleMenuItemClick(item[0], index)}>
                {item[1]}
                  </MenuItem>
                ))}
                
            </Menu>
        </React.Fragment>
    </div>
  );
}