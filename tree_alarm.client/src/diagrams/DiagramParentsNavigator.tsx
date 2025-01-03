﻿import * as React from 'react';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';

import * as DiagramsStore from '../store/DiagramsStates';
import { TreeMarker } from '../store/Marker';
import { useAppDispatch } from '../store/configureStore';

interface IDiagramParentsNavigator {
  parent_list: TreeMarker[] | null;
  parent_id: string | null;
}
export default function DiagramParentsNavigator(props: IDiagramParentsNavigator) {

  const parent_list = props.parent_list;

  let curMarker = parent_list?.find((element: any) => {
    return element?.id == props?.parent_id;
  }) ?? null;

  const appDispatch = useAppDispatch();

  const handleChange = (event: React.SyntheticEvent, newValue: TreeMarker | null) => {
    if (newValue?.id == null) {
      appDispatch(DiagramsStore.update_single_diagram_locally(null));
      return;
    }
    appDispatch(DiagramsStore.fetchGetDiagramContent(newValue?.id));
  };

  if (parent_list == null) {
    return null;
  }
  return (
    <Box sx={{
      backgroundColor: '#bbbbbb',
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