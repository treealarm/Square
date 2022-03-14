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

  const markers = useSelector((state) => state?.treeStates?.markers);

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
    (event: React.SyntheticEvent, nodeIds: Array<string> | string) => {
      console.log("onNodeToggle", nodeIds);
    }, [])

    return (
      <TreeView
        aria-label="file system navigator"
        defaultCollapseIcon={<ExpandMoreIcon />}
        defaultExpandIcon={<ChevronRightIcon />}
        sx={{ height: 240, flexGrow: 1, maxWidth: 400, overflowY: 'auto' }}
        onNodeSelect={onNodeSelect}
        onNodeToggle={onNodeToggle}
      >
        {markers?.map((marker, index) =>
          <TreeItem nodeId={marker.id} label={marker.name}/>
        )}
      </TreeView>
    );
}