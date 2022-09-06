import * as React from 'react';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { ApplicationState } from '../store';
import DeleteIcon from '@mui/icons-material/Delete';
import { KeyValueDTO, ObjPropsSearchDTO } from '../store/Marker';
import { Box, IconButton, List, ListItem } from '@mui/material';
import { useCallback } from 'react';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function PropertyFilter(props:any) {

  function handleChangePropName(e: any) {
    const { target: { id, value } } = e;

    let copy = Object.assign({}, props.propsFilter);
    if (copy == null) {
      return;
    }

    const first = copy.props.at(id);

    if (first != null) {
      first.prop_name = value;
    }
    props.setPropsFilter(copy);
  };

  function handleChangePropVal(e: any) {
    const { target: { id, value } } = e;

    let copy = Object.assign({}, props.propsFilter);
    if (copy == null) {
      return;
    }

    const first = copy.props.at(id);

    if (first != null) {
      first.str_val = value;
    }

    props.setPropsFilter(copy);
  };

  const deleteProperty = useCallback(
    (e, item: KeyValueDTO) => {
      let copy = Object.assign({}, props.propsFilter);
      copy.props = copy.props.filter((obj: KeyValueDTO) => { return obj !== item; });
      props.setPropsFilter(copy);
    }, [props.propsFilter]);

  return (
    <Box
      sx={{
        width: '100%',
        maxWidth: 460,
        bgcolor: 'background.paper',
        overflow: 'auto',
        height: '100%',
        border: 1
      }}>
    <List>
      {
        props.propsFilter?.props?.map((item: KeyValueDTO, index: { toString: () => string; }) =>
          
          <ListItem>
            <Stack spacing={2}
              
              sx={{
                m: 1
              }}>
              <TextField size="small"
                fullWidth
                id={index.toString()} label="prop_name"
                value={item.prop_name}
                onChange={handleChangePropName} />
              <TextField size="small"
                fullWidth
                id={index.toString()} label="prop_val"
                value={item.str_val}
                onChange={handleChangePropVal} />
              </Stack>
              <IconButton aria-label="addProp" size="large" onClick={(e) => deleteProperty(e, item)}>
                <DeleteIcon fontSize="inherit" />
              </IconButton>
            </ListItem>
        )
      }
      </List>
    </Box>
  );
}

