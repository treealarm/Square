import React, { useEffect, useCallback, useState } from 'react';
import { useAppDispatch } from '../store/configureStore';
import { getByParent, getById } from '../store/TreeStates'; // Убедитесь, что вы импортируете правильно
import { TreeMarker } from '../store/Marker';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Popover from '@mui/material/Popover';
import Button from '@mui/material/Button';
import Checkbox from '@mui/material/Checkbox';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { Box } from '@mui/material';

interface ObjectSelectorProps {
  selectedId: string | null;
  onSelect: (id: string | null) => void;
}

export function ObjectSelector({ selectedId, onSelect }: ObjectSelectorProps) {
  const dispatch = useAppDispatch();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [items, setItems] = useState<TreeMarker[]>([]);
  const [currentParentId, setCurrentParentId] = useState<string | null>(null);

  // Обновленный fetchTreeItems с использованием unwrap
  const fetchTreeItems = useCallback(async (parentId: string | null) => {
    try {
      const response = await dispatch(getByParent({ parent_id: parentId, start_id: null, end_id: null })).unwrap();
      setItems(response.children); // Предположим, что `response` содержит `children`
    } catch (error) {
      console.error('Failed to fetch tree items:', error);
    }
  }, [dispatch]);

  useEffect(() => {
    const fetchData = async () => {
      if (selectedId) {
        try {
          const response = await dispatch(getById(selectedId)).unwrap();
          setCurrentParentId(response.parent_id);
          fetchTreeItems(response.parent_id);
        } catch (error) {
          console.error('Failed to fetch item by ID:', error);
        }
      } else {
        fetchTreeItems(null);
      }
    };

    fetchData();
  }, [selectedId, fetchTreeItems, dispatch]);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleSelect = (item: TreeMarker) => () => {
    onSelect(item.id);
    handleClose();
  };

  const open = Boolean(anchorEl);
  const id = open ? 'simple-popover' : undefined;

  return (
    <>
      <Button aria-describedby={id} variant="contained" onClick={handleClick}>
        Select Item
      </Button>
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
        <Box sx={{ width: 300 }}>
          <List>
            {items.map((item) => (
              <ListItem key={item.id} disablePadding>
                <ListItemButton
                  selected={selectedId === item.id}
                  onClick={handleSelect(item)}
                >
                  <ListItemIcon>
                    <Checkbox
                      edge="start"
                      checked={selectedId === item.id}
                      tabIndex={-1}
                      disableRipple
                    />
                  </ListItemIcon>
                  <ListItemText primary={item.name} />
                  {item.has_children && <ChevronRightIcon />}
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        </Box>
      </Popover>
    </>
  );
}
