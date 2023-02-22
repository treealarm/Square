import * as React from 'react';

import { useSelector } from "react-redux";
import * as RightsStore from '../store/RightsStates';
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box} from '@mui/material';
import { IObjectRightsDTO, IObjectRightValueDTO} from '../store/Marker';
import { useAppDispatch } from '..';
import RoleRightSelector from './RoleRightSelector';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectRights() {

  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const rights = useSelector((state: ApplicationState) => state?.rightsStates?.rights);
  const rightValues = useSelector((state: ApplicationState) => state?.rightsStates?.all_rights);

  var myRight = rights?.find(r => r.id == selected_id);

  if (myRight == null) {
    myRight =
    {
      id: selected_id,
      rights: []
    };
  }

  React.useEffect(() => {
    appDispatch(RightsStore.fetchAllRoles());
    appDispatch(RightsStore.fetchAllRightValues());
  }, []);

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
        <ListItem key="FirstItemOfRightList">
          {selected_id}
        </ListItem>
        {
          myRight?.rights?.map((item: IObjectRightValueDTO, index: any) =>
            <ListItem key={index}>
              <RoleRightSelector right_values={rightValues} />
            </ListItem>
          )
        }
      </List>
    </Box>
  );
}