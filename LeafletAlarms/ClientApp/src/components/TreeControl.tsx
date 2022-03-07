import * as React from 'react';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { useEffect, useCallback } from 'react';
import { TreeItem, TreeView } from '@mui/lab';


export function TreeControl() {

  useEffect(() => {
    //console.log('ComponentDidMount');
  }, []);

  const onNodeSelect = useCallback(
    (event: React.SyntheticEvent, nodeIds: Array<string> | string) => {
      console.log("onNodeSelect", nodeIds);
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
        <TreeItem nodeId="1" label="Applications">
          <TreeItem nodeId="2" label="Calendar" />
        </TreeItem>
        <TreeItem nodeId="5" label="Documents">
          <TreeItem nodeId="10" label="OSS" />
          <TreeItem nodeId="6" label="MUI">
            <TreeItem nodeId="8" label="index.js" />
          </TreeItem>
        </TreeItem>
      </TreeView>
    );
}