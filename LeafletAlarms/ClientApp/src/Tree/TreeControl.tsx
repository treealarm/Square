import * as React from 'react';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { useEffect, useCallback, useState} from 'react';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';

import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import CommentIcon from '@mui/icons-material/Comment';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function TreeControl() {

  const dispatch = useDispatch();

  useEffect(() => {
    console.log('ComponentDidMount TreeControl');
    dispatch(TreeStore.actionCreators.getByParent(null));
  }, []);

  const [levelUp, setLevelUp] = useState(null);

  const markers = useSelector((state) => state?.treeStates?.markers);
  const parent_id = useSelector((state) => state?.treeStates?.parent_id);

  const [checked, setChecked] = React.useState([]);

  const handleSelect = useCallback((selected_id) => () => {
    dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
  }, []);

  const drillDown = useCallback((selected_id) => () => {
    setLevelUp(parent_id);
    dispatch(TreeStore.actionCreators.getByParent(selected_id));
  }, []);

  const handleChecked = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {

    var selected_id = event.target.id;
    const currentIndex = checked.indexOf(selected_id);
    const newChecked = [...checked];

    if (event.target.checked) {
      newChecked.push(selected_id);
    } else {
      newChecked.splice(currentIndex, 1);
    }

    setChecked(newChecked);

    dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
  }, []);


    return (
      <List sx={{ width: '100%', maxWidth: 360, bgcolor: 'background.paper' }}>
        {
          parent_id != null &&
          <ListItem>
            <ListItemText id={levelUp} primary='UP'>
            </ListItemText>
          </ListItem>
        }
        
        {
          markers?.map((marker, index) =>
            <ListItem
              key={marker.id}
              disablePadding
              secondaryAction={
                marker.has_children &&
                <IconButton edge="end" aria-label="comments">
                  <CommentIcon onClick={drillDown(marker.id)}/>
                </IconButton>
              }
            >
              <ListItemButton role={undefined} onClick={handleSelect(marker.id)} dense>
                <ListItemIcon>
                  <Checkbox
                    edge="start"
                    checked={checked.indexOf(marker.id) !== -1}
                    tabIndex={-1}
                    disableRipple
                    inputProps={{ 'aria-labelledby': marker.id }}
                    id={marker.id}
                    onChange={handleChecked}
                  />
                </ListItemIcon>
                <ListItemText id={marker.id} primary={marker.name} />
              </ListItemButton>
          </ListItem>
        )}
      </List>
    );
}