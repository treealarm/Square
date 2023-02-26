import * as React from 'react';

import { useSelector } from "react-redux";
import * as RightsStore from '../store/RightsStates';
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, Button, ButtonGroup, IconButton, TextField } from '@mui/material';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import { DeepCopy, IObjectRightsDTO, IObjectRightValueDTO} from '../store/Marker';
import { useAppDispatch } from '..';
import RoleRightSelector from './RoleRightSelector';
import SaveIcon from '@mui/icons-material/Save';
import { useCallback } from 'react';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectRights() {

  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const rights = useSelector((state: ApplicationState) => state?.rightsStates?.rights);
  const rightValues = useSelector((state: ApplicationState) => state?.rightsStates?.all_rights);
  const roleValues = useSelector((state: ApplicationState) => state?.rightsStates?.all_roles);
  const user = useSelector((state: ApplicationState) => state?.rightsStates?.user);

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
  }, [user]);

  React.useEffect(() => {
    appDispatch(RightsStore.fetchRightsByIds([selected_id]));
  }, [selected_id]);

  const handleSave = useCallback(() => {
    if (rights != null && rights.length > 0) {
      var copy_rights = DeepCopy(rights);
      appDispatch(RightsStore.updateRights(copy_rights));
    }
  }, [rights]);

  function addRoleRight
    (e: any) {
    var copy_right = DeepCopy(myRight);
    var newRightValue: IObjectRightValueDTO =
    {
      role: '',
      value: 0
    }
    copy_right.rights = [...copy_right.rights, newRightValue];
    appDispatch(RightsStore.set_rights([copy_right]));
  };

  function onChangeRoleValue(roleRight: IObjectRightValueDTO, index: number) {
    var copy_right = DeepCopy(myRight);
    if (roleRight == null) {
      copy_right.rights.splice(index, 1);
    }
    else {
      copy_right.rights[index] = roleRight;
    }
    
    appDispatch(RightsStore.set_rights([copy_right]));
  }
  var rolesAvailable = roleValues.filter(v => myRight?.rights.findIndex(t => t.role == v) < 0);

  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List>
        <ListItem>
        <ButtonGroup variant="contained" aria-label="right pannel">
          <IconButton aria-label="save" size="medium" onClick={handleSave}>
            <SaveIcon fontSize="inherit"></SaveIcon>
          </IconButton> 

          <IconButton color="primary"
            aria-label="addProp"
            size="medium"
            onClick={(e: any) => addRoleRight(e)}>
            <LibraryAddIcon fontSize="inherit" />
          </IconButton>
        </ButtonGroup> 
        </ListItem>

        <ListItem>
          <TextField
            fullWidth
            label='Id'
            size="small"
            value={selected_id}
            inputProps={{ readOnly: true }}>
          </TextField>
        </ListItem>
        {
          myRight?.rights?.map((item: IObjectRightValueDTO, index: any) =>
            <ListItem key={index}>
              <RoleRightSelector
                right_values={rightValues}
                role_values={rolesAvailable}
                cur_value={item}
                index={index}
                onChangeRoleValue={onChangeRoleValue}
              />
            </ListItem>
          )
        }
      </List>
    </Box>
  );
}