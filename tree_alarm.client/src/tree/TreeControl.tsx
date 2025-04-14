/* eslint-disable no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import { DeepCopy, IIntegroTypeDTO, IObjProps, IUpdateIntegroObjectDTO, TreeMarker } from '../store/Marker';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { Box, Menu, MenuItem, Toolbar, Tooltip } from '@mui/material';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AddIcon from '@mui/icons-material/Add';
import MenuOpenIcon from '@mui/icons-material/MenuOpen';
import TabControl from './TabControl';
import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import * as IntegroStore from '../store/IntegroStates';
import * as ObjPropsStore from '../store/ObjPropsStates';
import { useAppDispatch } from "../store/configureStore";
import { RequestedState } from '../store/TreeStates';
import RefreshIcon from '@mui/icons-material/Refresh';

export function TreeControl() {
  const appDispatch = useAppDispatch();

  const parentBounds = useSelector((state: ApplicationState) => state.treeStates?.parentBounds || {});

  const markers = useSelector((state: ApplicationState) => state.treeStates?.children);
  const parentMarkerId = useSelector((state: ApplicationState) => state.treeStates?.requestedState?.parent_id);
  const requestedState: RequestedState|null = useSelector((state: ApplicationState) => state.treeStates?.requestedState ?? null);
  const user = useSelector((state: ApplicationState) => state.rightsStates?.user);
  const reduxSelectedId = useSelector((state: ApplicationState) => state.guiStates?.selected_id);
  const requestTreeUpdate = useSelector((state: ApplicationState) => state.guiStates?.requestedTreeUpdate);

  const objectIntegroType = useSelector((state: ApplicationState) => state?.integroStates?.integroType ?? null);

  const childTypes = objectIntegroType?.children ?? [];

  const startEndBounds = parentMarkerId ? parentBounds[parentMarkerId] : parentBounds[''];

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [debounceTimeout, setDebounceTimeout] = useState<any | null>(null);

  const getTreeItemsByParent = useCallback((requestedState: RequestedState | null) => {
    if (requestedState) {
      requestedState = DeepCopy(requestedState);
    }
    appDispatch(TreeStore.setParentIdLocally(requestedState));
  }, [appDispatch]);
 

  useEffect(() => {
    getTreeItemsByParent({
      parent_id: parentMarkerId ?? null,
      start_id: startEndBounds?.start_id,
      end_id: startEndBounds?.end_id
    });
  }, [user, getTreeItemsByParent]);

  useEffect(() => {
    // Если есть предыдущий таймер, его нужно очистить
    if (debounceTimeout) {
      clearTimeout(debounceTimeout);
    }

    // Создаём новый таймер
    const timeout = setTimeout(() => {
      appDispatch(TreeStore.fetchByParent(requestedState));
    }, 1000); // задержка 1 секунда TODO сделать компонент, аккумулирующий изменения конфигуры

    // Сохраняем текущий таймер, чтобы его можно было очистить на следующем рендере
    setDebounceTimeout(timeout);

    // Очистка таймера при размонтировании компонента
    return () => {
      if (timeout) clearTimeout(timeout);
    };
  }, [requestedState, requestTreeUpdate]);

  const [checked, setChecked] = React.useState<Set<string>>(new Set());

  const handleChecked = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedId = event.target.id;
    const newChecked = new Set(checked);
    if (event.target.checked) {
      newChecked.add(selectedId);
    } else {
      newChecked.delete(selectedId);
    }
    setChecked(newChecked);
    appDispatch(GuiStore.checkTreeItem(Array.from(newChecked)));
  };

  const selectItem = (selectedMarker: TreeMarker | null) => {
    const selectedId = selectedMarker?.id === reduxSelectedId ? null : selectedMarker?.id ?? null;
    appDispatch(GuiStore.selectTreeItem(selectedId));

    if (selectedId) {
      appDispatch(IntegroStore.fetchObjectIntegroType(selectedId));
    } else {
      // Очищаем типы, если ничего не выбрано
      appDispatch(IntegroStore.set_objectIntegroType(null));
    }
  };

  
  const handleSelect = (selected: TreeMarker) => () => selectItem(selected);

  const drillDown = (selectedMarker: TreeMarker | null) => () => {
    selectItem(null);
    const my_bounds = selectedMarker?.id ? parentBounds[selectedMarker?.id] : parentBounds[''];

    getTreeItemsByParent({
      parent_id: selectedMarker?.id ?? null,
      start_id: my_bounds?.start_id,
      end_id: my_bounds?.end_id
    });
  };

  const onNavigate = (next: boolean) => {
    getTreeItemsByParent({
      parent_id: parentMarkerId ??  null,
      start_id: next ? startEndBounds?.end_id : null,
      end_id: next ? null : startEndBounds?.start_id
    });
  };

  const refreshTree = () => {
    const my_bounds = parentMarkerId ? parentBounds[parentMarkerId] : parentBounds[''];
    getTreeItemsByParent({
      parent_id: parentMarkerId ?? null,
      start_id: my_bounds?.start_id ?? null,
      end_id: null
    });
  };


  const addChildItem = (type?: string|null) => {

    let copy: IObjProps = {
      id: null,
      name: 'new object',
      parent_id: reduxSelectedId
    }   

    if (type && objectIntegroType) {
      copy.name = 'new ' + type;
      let new_obj: IUpdateIntegroObjectDTO = {
        obj: copy,
        integro: {
          i_name: objectIntegroType.i_name,
          i_type: objectIntegroType.i_type
        }
      }

      appDispatch(IntegroStore.updateIntegroObject(new_obj));
    }
    else {
      appDispatch(ObjPropsStore.updateObjProps(copy));
    }
    const my_bounds = parentMarkerId ? parentBounds[parentMarkerId] : parentBounds[''];
    getTreeItemsByParent({
      parent_id: parentMarkerId ?? null,
      start_id: my_bounds?.start_id ?? null,
      end_id: null
    });
  };

  return (
    <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
      <TabControl />
      <Box sx={{ flexGrow: 1, backgroundColor: 'lightgray' }}>
        <Toolbar variant="dense">

          <Tooltip title="Go to previous page">
            <IconButton onClick={() => onNavigate(false)}>
              <ArrowBackIcon />
            </IconButton>
          </Tooltip>
          <Box sx={{ flexGrow: 1 }} />
          {reduxSelectedId == null ? (
            <Tooltip title="Add new object">
              <IconButton onClick={()=>addChildItem(null)}>
                <AddIcon />
              </IconButton>
            </Tooltip>
          ) : null}
          <Tooltip title="Refresh tree">
            <IconButton onClick={refreshTree}>
              <RefreshIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Go to next page">
            <IconButton onClick={() => onNavigate(true)}>
              <ArrowForwardIcon />
            </IconButton>
          </Tooltip>
        </Toolbar>
      </Box>
      <Box sx={{ width: '100%', height: '100%', overflow: 'auto' }}>
        <List dense sx={{ minHeight: '100%', width: '100%' }}>
          {markers?.map((marker) => (
            <ListItem key={marker.id} disablePadding secondaryAction={
              <>
                {marker.has_children && (
                  <IconButton size="small" edge="end" aria-label="drill_down" onClick={drillDown(marker)}>
                    <ChevronRightIcon />
                  </IconButton>
                )}

                {reduxSelectedId === marker.id && (
                  <>
                    <Tooltip title="Add new child">
                      <IconButton size="small" edge="end" aria-label="add_child" onClick={() => addChildItem(null)}>
                        <AddIcon />
                      </IconButton>
                    </Tooltip>

                    {/* Кнопка для открытия меню */}
                    {childTypes.length > 0 && (
                      <>
                        <IconButton
                          size="small"
                          edge="end"
                          aria-label="add_typed_child"
                          onClick={(e) => setAnchorEl(e.currentTarget)}
                        >
                          <MenuOpenIcon />
                        </IconButton>

                        <Menu
                          anchorEl={anchorEl}
                          open={Boolean(anchorEl)}
                          onClose={() => setAnchorEl(null)}
                        >
                          {childTypes.map((type) => (
                            <MenuItem
                              key={type.child_i_type}
                              onClick={() => {
                                addChildItem(type.child_i_type);
                                setAnchorEl(null); // Закрыть меню после клика
                              }}
                            >
                              Add {type.child_i_type}
                            </MenuItem>
                          ))}
                        </Menu>
                      </>
                    )}
                  </>
                )}



              </>
            }>
              <ListItemButton selected={reduxSelectedId === marker.id} onClick={handleSelect(marker)}>
                <ListItemIcon>
                  <Checkbox
                    size="small"
                    edge="start"
                    checked={checked.has(marker.id)}
                    tabIndex={-1}
                    disableRipple
                    id={marker.id}
                    onChange={handleChecked}
                  />
                </ListItemIcon>
                <ListItemText primary={marker.name} />
              </ListItemButton>
            </ListItem>
          ))}
        </List>
      </Box>
    </Box>
  );
}
