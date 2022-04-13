import * as React from 'react';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';

import { useDispatch, useSelector } from 'react-redux';

import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { TreeMarker } from '../store/Marker';
import { useCallback } from 'react';

export default function TabControl() {

  const parent_list = useSelector((state) => state?.treeStates?.parent_list);

  const dispatch = useDispatch();

  const handleChange = (event: React.SyntheticEvent, newValue: any) => {
    dispatch(TreeStore.actionCreators.getByParent(newValue));
  };
  

  return (
    <Box sx={{ border: 1 }}>
      <Tabs
        value={parent_list.slice(-1)[0]}
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