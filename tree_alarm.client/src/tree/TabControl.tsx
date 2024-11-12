import * as React from 'react';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';

import { useSelector } from 'react-redux';

import * as TreeStore from '../store/TreeStates';
import { TreeMarker } from '../store/Marker';
import { ApplicationState } from '../store';
import { useAppDispatch } from '../store/configureStore';

export default function TabControl() {

  const treeState = useSelector((state: ApplicationState) => state?.treeStates);
  const parentMarkerId = useSelector((state: ApplicationState) => state.treeStates?.requestedState?.parent_id);
 
  const parentBounds = useSelector((state: ApplicationState) => state.treeStates?.parentBounds || {});
  const parent_list = treeState?.parents ?? [];


  let curMarker = parent_list.find((element: any) => {
    return element?.id == parentMarkerId;
  }) ?? null;

  curMarker = curMarker??null;
  const appDispatch = useAppDispatch();

  const handleChange = (event: React.SyntheticEvent, newValue: TreeMarker | null) => {
    const my_bounds = newValue?.id ? parentBounds[newValue?.id] : parentBounds[''];
    appDispatch(TreeStore.setParentIdLocally({
      parent_id: newValue?.id ?? null,
      start_id: my_bounds.start_id,
      end_id: my_bounds.end_id
    }));
  };
  

  return (
    <Box sx={{
      backgroundColor: '#dca',
      flexGrow: 1,
      display: 'flex'
    }}
      >
      <Tabs
        value={curMarker}
        onChange={handleChange}        
        variant='scrollable'
        scrollButtons
        allowScrollButtonsMobile
        selectionFollowsFocus
      >
        {
          parent_list?.map((marker, index) =>
            <Tab
              label={marker != null ? marker?.name : 'ROOT'}
              value={marker}
              key={index} />
          )
        }
      </Tabs>
    </Box>
  );
}