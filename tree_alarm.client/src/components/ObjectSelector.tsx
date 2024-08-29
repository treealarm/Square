import React, { useEffect, useCallback, useState } from 'react';
import { getByParent, getById } from '../store/TreeStates'; // Убедитесь, что вы импортируете правильно
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
  // eslint-disable-next-line no-unused-vars
  onSelect: (id: string | null) => void;
}

export function ObjectSelector({ selectedId, excludeId, onSelect }: ObjectSelectorProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [items, setItems] = useState<TreeMarker[]>([]);
  const [parents, setParents] = useState<TreeMarker[]>([]);
  const [currentParentId, setCurrentParentId] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [selectedItem, setSelectedItem] = useState<string | null>(selectedId);

  
  // Обновленный fetchTreeItems с использованием unwrap
  const fetchTreeItems = useCallback(async (parentId: string | null) => {
    setLoading(true);
    try {
      const response = await getByParent(parentId, null, null);
      var availableForSelection = response.children?.filter(marker => marker.id !== excludeId) ?? null;

      setItems(availableForSelection ?? []); // Предположим, что `response` содержит `children`
      setParents(response.parents ?? []); // Предположим, что `response` содержит `parents`
      setCurrentParentId(response.parent_id ?? null);
    } catch (error) {
      console.error('Failed to fetch tree items:', error);
    } finally {
      setLoading(false);
    }
  }, [excludeId]);

  useEffect(() => {
    const fetchData = async () => {
      if (selectedId) {
        try {
          const response = await getById(selectedId);
          fetchTreeItems(response.parent_id ?? null);
        } catch (error) {
          console.error('Failed to fetch item by ID:', error);
        }
      } else {
        fetchTreeItems(null);
      }
    };

    fetchData();
    
  }, [selectedId, fetchTreeItems]);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
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
    var id = selectedItem;
    onSelect(id);
    setTimeout(() => {
      handleClose();
    }, 0);
  };

  const handleCancel = () => {
    setSelectedItem(null);
    handleClose();
  };

  const handleExpand = (item: TreeMarker) => () => {
    fetchTreeItems(item.id);
  };

  const open = Boolean(anchorEl);
  const id = open ? 'simple-popover' : undefined;

  return (
    <>
      <IconButton aria-label="select item" onClick={handleClick}>
        <CheckCircleIcon />
      </IconButton>
      <Popover
        id={id}
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'left',
        }}
      >
        <Box sx={{ width: 300, padding: 2, display: 'flex', flexDirection: 'column', height: 400 }}>
          {/* Tabs for parents */}
          <Tabs
            value={currentParentId}
            onChange={handleParentChange}
            variant="scrollable"
            scrollButtons="auto"
            allowScrollButtonsMobile
            sx={{ marginBottom: 1 }}
          >
            {parents.map((parent) => (
              <Tab
                key={parent.id}
                label={parent.name}
                value={parent.id}
              />
            ))}
            <Tab
              label="Root"
              value={null}
            />
          </Tabs>

          {/* List of items */}
          <Box sx={{ flexGrow: 1, overflowY: 'auto' }}>
            {loading ? (
              <CircularProgress />
            ) : (
              <List>
                {items.map((item) => (
                  <ListItem key={item.id} disablePadding>
                    <ListItemButton
                      selected={selectedItem === item.id}
                      onClick={handleSelect(item)}
                    >
                      <ListItemIcon>
                        <Checkbox
                          edge="start"
                          checked={selectedItem === item.id}
                          tabIndex={-1}
                          disableRipple
                        />
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

          {/* Action buttons */}
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', marginTop: 2 }}>
            <Button onClick={handleCancel} sx={{ marginRight: 1 }}>
              Cancel
            </Button>
            <Button onClick={handleOk} variant="contained">
              OK
            </Button>
          </Box>
        </Box>
      </Popover>
    </>
  );
}
