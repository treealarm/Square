import * as React from 'react';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import ListItemIcon from '@mui/material/ListItemIcon';
import Divider from '@mui/material/Divider';


import { IconButton } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';


import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { useAppDispatch } from '../store/configureStore';
import * as PanelsStore from '../store/PanelsStates';
import { DeepCopy, EPanelType, IPanelTypes } from '../store/Marker';
import { PanelIcon } from './PanelIcon';

export default function PanelSwitch(props: { panelType: EPanelType }) {

  const appDispatch = useAppDispatch();
  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleSelect = (text: string, menuItem: string, panType: EPanelType) => {
    setAnchorEl(null);

    var exist = panels.find(e => e.panelId == menuItem && e.panelType == panType);

    if (exist) {
      var removed = panels.filter(e => e.panelId != menuItem);
      appDispatch(PanelsStore.set_panels(removed));
    }
    else {
      var newPanels = DeepCopy(panels);

      newPanels = newPanels.filter(e => e.panelType != panType);
      newPanels.push(
        {
          panelId: menuItem,
          panelValue: text,
          panelType: panType
        });
      console.log(JSON.stringify(newPanels));
      appDispatch(PanelsStore.set_panels(newPanels));
    }
  };

  var curPannels = IPanelTypes.panels.filter(p => p.panelType == props.panelType);

  return (
    <React.Fragment>
      <IconButton
        size="large"
        edge="start"
        color="inherit"
        aria-label="menu"
        sx={{ mr: 2}}
        onClick={handleClick}
      >
        <MenuIcon />
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        id="account-menu"
        open={open}
        onClose={handleClose}
        onClick={handleClose}
        PaperProps={{
          elevation: 0,
          sx: {
            overflow: 'visible',
            filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.32))',
            mt: 1.5,
            '& .MuiAvatar-root': {
              width: 32,
              height: 32,
              ml: -0.5,
              mr: 1,
            },
            '&:before': {
              content: '""',
              display: 'block',
              position: 'absolute',
              top: 0,
              right: 14,
              width: 10,
              height: 10,
              bgcolor: 'background.paper',
              transform: 'translateY(-50%) rotate(45deg)',
              zIndex: 0,
            },
          },
        }}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
      >
        <Divider/>
        {
          curPannels.map((datum) => 
            <MenuItem
              key={"PanelSwith:" + datum.panelId }
              onClick={() => handleSelect(
                datum.panelValue, datum.panelId, datum.panelType)}
              selected={panels.find(p => p.panelId == datum.panelId )!= null}>
              <ListItemIcon>
                <PanelIcon panelId={datum.panelId} />
              </ListItemIcon>
              {datum.panelValue}
            </MenuItem>
          )
        }
      </Menu>

    </React.Fragment>
  );
}