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
  const parent_list = treeState?.parents ?? [];

  let curMarker = parent_list.find((element: any) => {
    return element?.id == treeState?.parent_id;
  }) ?? null;

  curMarker = curMarker??null;
  const appDispatch = useAppDispatch();

  const handleChange = (event: React.SyntheticEvent, newValue: TreeMarker | null) => {
    appDispatch(TreeStore.setParentIdLocally(newValue?.id ?? null));
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