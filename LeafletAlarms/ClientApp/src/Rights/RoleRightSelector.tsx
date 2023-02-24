import * as React from 'react';
import Checkbox from '@mui/material/Checkbox';
import TextField from '@mui/material/TextField';
import Autocomplete, { AutocompleteChangeDetails, AutocompleteChangeReason } from '@mui/material/Autocomplete';
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank';
import CheckBoxIcon from '@mui/icons-material/CheckBox';
import { DeepCopy, IObjectRightValueDTO, IRightValuesDTO } from '../store/Marker';
import { Box, FormControl, IconButton, InputLabel, List, ListItem, MenuItem, OutlinedInputProps, Select, SelectChangeEvent } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';

const icon = <CheckBoxOutlineBlankIcon fontSize="small" />;
const checkedIcon = <CheckBoxIcon fontSize="small" />;

export default function RoleRightSelector(properties: any) {

  var right_values: IRightValuesDTO[] = properties.right_values;
  var role_values: string[] = properties.role_values;

  if (right_values == null) {
    right_values = [];
  }

  var cur_value: IObjectRightValueDTO = properties.cur_value;

  role_values = [...role_values, cur_value.role];

  var cur_right_values: IRightValuesDTO[] =
    right_values.filter(v => (v.rightValue & cur_value.value))



  function handleChangeRole(event: SelectChangeEvent) {
    const { target: { value } } = event;

    var cur_value_copy = DeepCopy(cur_value);

    cur_value_copy.role = value;
    properties.onChangeRoleValue(cur_value_copy, properties.index);
  };

  function handleChangeRights(
    event: React.SyntheticEvent,
    valueTag: IRightValuesDTO[],
    reason: any,
    details?: any
  ) {
    var cur_value_copy = DeepCopy(cur_value);
    var val = 0;
    valueTag.forEach((element) => {
      val = val | element.rightValue;
    });
    cur_value_copy.value = val;
    properties.onChangeRoleValue(cur_value_copy, properties.index);
  };

  function deleteMe
    (e: any) {
    properties.onChangeRoleValue(null, properties.index);
  };

  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>
      <List key="ListRightSelector">
        <ListItem key={cur_value.role}>
          <Select
            style={{ width: '100%' }}
            labelId="select_role"
            id={cur_value.role}
            value={cur_value.role}
            label="group id"
            onChange={handleChangeRole}
            size="small"
          >
            {
              role_values.map((item: string) =>
                <MenuItem value={item}>{item}</MenuItem>
              )
            }
          </Select>
          <IconButton color="primary"
            aria-label="addProp"
            size="medium"
            onClick={(e: any) => deleteMe(e)}          >

            <DeleteIcon fontSize="inherit"

            />

          </IconButton>
        </ListItem>

        <ListItem key={cur_value.value}>
          <FormControl fullWidth>
            <Autocomplete
              multiple
              id="checkboxes-tags-right"
              options={right_values}
              disableCloseOnSelect
              getOptionLabel={(option) => option.rightName}
              value={cur_right_values}
              onChange={handleChangeRights}
              renderOption={(props, option, { selected }) => (
                <li {...props}>
                  <Checkbox
                    icon={icon}
                    checkedIcon={checkedIcon}
                    style={{ marginRight: 8 }}
                    checked={selected}
                  />
                  {option.rightName}
                </li>
              )}
              style={{ width: '100%' }}
              renderInput={(params) => (
                <TextField {...params} label="rights" placeholder="Rights" />
              )}
            />

          </FormControl>
        </ListItem>
      </List>
       
    </Box>

  );
}
