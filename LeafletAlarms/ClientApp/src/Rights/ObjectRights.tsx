import * as React from 'react';

import { useSelector } from "react-redux";
import * as RightsStore from '../store/RightsStates';
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box} from '@mui/material';
import { IObjectRightsDTO} from '../store/Marker';
import { useAppDispatch } from '..';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectRights() {

  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const rights = useSelector((state: ApplicationState) => state?.rightsStates?.rights);

  React.useEffect(() => {
    appDispatch(RightsStore.fetchRightsByIds([selected_id]));
  }, [selected_id]);

  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List>
        {
          rights?.map((item: IObjectRightsDTO, index: any) =>
            <ListItem key={index}>
              {item.id}
            </ListItem>
          )
        }
      </List>
    </Box>
  );
}