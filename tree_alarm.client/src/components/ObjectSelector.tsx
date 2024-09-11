/* eslint-disable no-unused-vars */
import React, { useCallback, useState } from 'react';
import { getByParent, getById } from '../store/TreeStates';
import { TreeMarker } from '../store/Marker';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Popover from '@mui/material/Popover';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import Checkbox from '@mui/material/Checkbox';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { Box, IconButton, CircularProgress, Tabs, Tab, Button } from '@mui/material';

interface ObjectSelectorProps {
  selectedId: string | null;
  excludeId?: string | null;
  onSelect: (id: string | null) => void;
}

export function ObjectSelector({ selectedId, excludeId, onSelect }: ObjectSelectorProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [items, setItems] = useState<TreeMarker[]>([]);
  const [parents, setParents] = useState<TreeMarker[]>([]);
  const [currentParentId, setCurrentParentId] = useState<string | null>(selectedId);
  const [loading, setLoading] = useState<boolean>(false);
  const [selectedItem, setSelectedItem] = useState<string | null>(selectedId);

  const fetchTreeItems = useCallback(async (parentId: string | null) => {
    setLoading(true);
    try {
      const response = await getByParent(parentId, null, null);
      const availableForSelection = response.children?.filter(marker => marker.id !== excludeId) ?? [];

      setItems(availableForSelection);
      setParents(response.parents ?? []);
      setCurrentParentId(response.parent_id ?? null);
    } catch (error) {
      console.error('Failed to fetch tree items:', error);
    } finally {
      setLoading(false);
    }
  }, [excludeId]);

  // Handle click event to open the popover and fetch the tree one level above the selected object
  const handleClick = async (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
    if (selectedId) {
      try {
        const response = await getById(selectedId);
        const parentId = response.parent_id ?? null;
        fetchTreeItems(parentId);  // Fetch one level above the selected object
      } catch (error) {
        console.error('Failed to fetch item by ID:', error);
      }
    } else {
      fetchTreeItems(null);
    }
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleSelect = (item: TreeMarker) => () => {
    setSelectedItem(item.id);
  };

  const handleParentChange = (event: React.SyntheticEvent, newValue: string | null) => {
    fetchTreeItems(newValue);
  };

  const handleOk = () => {
    onSelect(selectedItem);
    handleClose();
  };

  const handleCancel = () => {
    setSelectedItem(null);
    handleClose();
  };

  const handleExpand = (item: TreeMarker) => () => {
    fetchTreeItems(item.id);
  };

  const open = Boolean(anchorEl);
  const popoverId = open ? 'simple-popover' : undefined;

  return (
    <>
      <IconButton aria-label="select item" onClick={handleClick}>
        <CheckCircleIcon />
      </IconButton>
      <Popover
        id={popoverId}
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
      >
        <Box sx={{ width: 300, padding: 2, display: 'flex', flexDirection: 'column', height: 400 }}>
          <Tabs
            value={currentParentId}
            onChange={handleParentChange}
            variant="scrollable"
            scrollButtons="auto"
            allowScrollButtonsMobile
            sx={{ marginBottom: 1 }}
          >
            {parents.map(parent => (
              <Tab key={parent.id} label={parent.name} value={parent.id} />
            ))}
            <Tab label="Root" value={null} />
          </Tabs>

          <Box sx={{ flexGrow: 1, overflowY: 'auto' }}>
            {loading ? (
              <CircularProgress />
            ) : (
              <List>
                {items.map(item => (
                  <ListItem key={item.id} disablePadding>
                    <ListItemButton selected={selectedItem === item.id} onClick={handleSelect(item)}>
                      <ListItemIcon>
                        <Checkbox edge="start" checked={selectedItem === item.id} tabIndex={-1} disableRipple />
                      </ListItemIcon>
                      <ListItemText primary={item.name} />
                      {item.has_children && (
                        <IconButton onClick={handleExpand(item)}>
                          <ChevronRightIcon />
                        </IconButton>
                      )}
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
            )}
          </Box>

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', marginTop: 2 }}>
            <Button onClick={handleCancel} sx={{ marginRight: 1 }}>
              Cancel
            </Button>
            <Button onClick={handleOk} variant="contained" disabled={loading}>
              OK
            </Button>
          </Box>
        </Box>
      </Popover>
    </>
  );
}
