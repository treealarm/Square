import * as React from 'react';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { useEffect, useCallback, useState} from 'react';
import { TreeItem, TreeView } from '@mui/lab';

import { useDispatch, useSelector, useStore } from "react-redux";
import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';

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

    const [selectedIndex, setSelectedIndex] = React.useState(1);

    const handleListItemClick = (event, index) => {
      setSelectedIndex(index);
    };

  const onNodeSelect = useCallback(
    (event: React.SyntheticEvent, nodeIds: Array<string> | string) => {

      console.log("onNodeSelect", nodeIds);

      if (typeof nodeIds === 'string') {
        dispatch(GuiStore.actionCreators.selectTreeItem(nodeIds));
      }
      else{
        dispatch(GuiStore.actionCreators.selectTreeItem(nodeIds[0]));
      }
    }, [])

  const onNodeToggle = useCallback(
    (event: React.SyntheticEvent, nodeIds: Array<string>) => {
      console.log("onNodeToggle", nodeIds);

      if (nodeIds.length > 0)
      {
        setLevelUp(parent_id);
        dispatch(TreeStore.actionCreators.getByParent(nodeIds[0]));
      }
    }, [])

    return (
      <TreeView
        aria-label="file system navigator"
        defaultCollapseIcon={<ExpandMoreIcon />}
        defaultExpandIcon={<ChevronRightIcon />}
        sx={{ height: 240, flexGrow: 1, maxWidth: 400, overflowY: 'auto' }}
        onNodeSelect={onNodeSelect}
        onNodeToggle={onNodeToggle}
        expanded={[]}
      >
        {
          parent_id != null &&
          <TreeItem nodeId={levelUp} label='UP'>
            <TreeItem nodeId='level_up_loading' label='Loading...'>
            </TreeItem>
          </TreeItem>
        }
        
        {
          markers?.map((marker, index) =>
          <TreeItem nodeId={marker.id} label={marker.name}>
            { 
              marker.has_children &&
                <TreeItem nodeId={marker.id + 'fake'} label='Loading...' />
            }
          </TreeItem>
        )}
      </TreeView>
    );
}