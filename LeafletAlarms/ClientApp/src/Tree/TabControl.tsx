import * as React from 'react';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';

import { useDispatch, useSelector } from 'react-redux';

import * as TreeStore from '../store/TreeStates';
import { TreeMarker } from '../store/Marker';
import { ApplicationState } from '../store';

export default function TabControl() {

  const treeState = useSelector((state: ApplicationState) => state?.treeStates);
  const parent_list = treeState.parents;

  let curMarker = parent_list.find((element: any) => {
    return element?.id == treeState?.parent_id;
  });

  curMarker = curMarker == undefined ? null : curMarker;
  const dispatch = useDispatch();

  const handleChange = (event: React.SyntheticEvent, newValue: TreeMarker|null) => {
    dispatch<any>(TreeStore.actionCreators.getByParent(newValue?.id, null, null));
  };
  

  return (
    <Box sx={{ border: 1 }}>
      <Tabs
        value={curMarker}
        onChange={handleChange}
        variant="scrollable"
        scrollButtons
        allowScrollButtonsMobile
        aria-label="scrollable force tabs example"
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