import * as React from 'react';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import ListItemIcon from '@mui/material/ListItemIcon';
import Divider from '@mui/material/Divider';
import QueryStatsIcon from '@mui/icons-material/QueryStats';

import { IconButton } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import DataObjectIcon from '@mui/icons-material/DataObject';
import SearchIcon from '@mui/icons-material/Search';
import SchemaIcon from '@mui/icons-material/Schema';
import LockPersonIcon from '@mui/icons-material/LockPerson';

import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { useAppDispatch } from '../store/configureStore';
import * as PanelsStore from '../store/PanelsStates';
import { DeepCopy, IPanelTypes } from '../store/Marker';

export default function PanelSwitch(props: { IsLeftPanel: boolean }) {

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

  const handleSelect = (text: string, menuItem: string, isLeft: boolean = false) => {
    setAnchorEl(null);
    var exist = panels.find(e => e.panelId == menuItem);

    if (exist) {
      var removed = panels.filter(e => e.panelId != menuItem);
      appDispatch(PanelsStore.set_panels(removed));
    }
    else {
      var newPanels = DeepCopy(panels);

      newPanels = newPanels.filter(e => e.IsLeft != isLeft);
      newPanels.push(
        {
          panelId: menuItem,
          panelValue: text,
          IsLeft: isLeft
        });
      console.log(JSON.stringify(newPanels));
      appDispatch(PanelsStore.set_panels(newPanels));
    }
  };

  var search_result = panels.find(e => e.panelId == IPanelTypes.search_result);
  var tree = panels.find(e => e.panelId == IPanelTypes.tree);

  var properties = panels.find(e => e.panelId == IPanelTypes.properties);
  var search = panels.find(e => e.panelId == IPanelTypes.search);
  var logic = panels.find(e => e.panelId == IPanelTypes.logic);
  var rights = panels.find(e => e.panelId == IPanelTypes.rights);

  var track_props = panels.find(e => e.panelId == IPanelTypes.track_props);

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
        <Divider />

        {
          props.IsLeftPanel ?
            <div>
              <MenuItem
                onClick={(e: any) => handleSelect(
                  IPanelTypes.treeName, IPanelTypes.tree, true)}
                selected={tree != null}>
                <ListItemIcon>
                  <AccountTreeIcon fontSize="small" />
                </ListItemIcon>
                {IPanelTypes.treeName}
              </MenuItem>

              <MenuItem onClick={(e: any) => handleSelect(
                IPanelTypes.search_resultName, IPanelTypes.search_result, true)}
                selected={search_result != null}>
                <ListItemIcon>
                  <QueryStatsIcon fontSize="small" />
                </ListItemIcon>
                {IPanelTypes.search_resultName}
              </MenuItem>
            </div>
            :
            <div>
              <MenuItem onClick={(e: any) => handleSelect(
                IPanelTypes.propertiesName, IPanelTypes.properties)}
                selected={properties != null}>
                <ListItemIcon>
                  <DataObjectIcon fontSize="small" />
                </ListItemIcon>
                {IPanelTypes.propertiesName}
              </MenuItem>

              <MenuItem onClick={(e: any) => handleSelect(
                IPanelTypes.searchName, IPanelTypes.search)}
                selected={search != null}>
                <ListItemIcon>
                  <SearchIcon fontSize="small" />
                </ListItemIcon>
                {IPanelTypes.searchName}
              </MenuItem>

              <MenuItem onClick={(e: any) => handleSelect(
                IPanelTypes.track_propsName, IPanelTypes.track_props)}
                selected={track_props != null}>
                <ListItemIcon>
                  <LockPersonIcon fontSize="small" />
                </ListItemIcon>
                {IPanelTypes.track_propsName}
              </MenuItem>

              <MenuItem onClick={(e: any) => handleSelect(
                IPanelTypes.logicName, IPanelTypes.logic)}
                selected={logic != null}>
                <ListItemIcon>
                  <SchemaIcon fontSize="small" />
                </ListItemIcon>
                {IPanelTypes.logicName}
              </MenuItem>

              <MenuItem onClick={(e: any) => handleSelect(
                IPanelTypes.rightsName, IPanelTypes.rights)}
                selected={rights != null}>
                <ListItemIcon>
                  <LockPersonIcon fontSize="small" />
                </ListItemIcon>
                {IPanelTypes.rightsName}
              </MenuItem>
            </div>
        }

      </Menu>

    </React.Fragment>
  );
}