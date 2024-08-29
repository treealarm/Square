import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import CloseIcon from "@mui/icons-material/Close";
import { DeepCopy, KeyValueDTO, ObjPropsSearchDTO } from '../store/Marker';
import { Box, IconButton, List, ListItem, Tooltip } from '@mui/material';

export function PropertyFilter(props:any) {

  function handleChangePropName(e: any) {
    const { target: { id, value } } = e;

    let copy = DeepCopy(props.propsFilter);

    if (copy == null) {
      return;
    }

    const first = copy.props.at(id);

    if (first != null) {
      first.prop_name = value;
    }
    props.setPropsFilter(copy);
  }

  function handleChangePropVal(e: any) {
    const { target: { id, value } } = e;

    let copy = DeepCopy(props.propsFilter);
    if (copy == null) {
      return;
    }

    const first = copy.props.at(id);

    if (first != null) {
      first.str_val = value;
    }

    props.setPropsFilter(copy);
  }

  function deleteProperty
    (e: any, index: any){
      let copy = DeepCopy(props.propsFilter) as ObjPropsSearchDTO;
      copy.props.splice(index, 1);
      props.setPropsFilter(copy);
    }

  return (
    <Box
      sx={{
        width: '100%',
        overflow: 'auto',
        height: '100%'
      }}>
      <List dense sx={{ maxHeight: "250px", overflow: 'auto', width: "100%" }}>
      {
        props.propsFilter?.props?.map((item: KeyValueDTO, index: { toString: () => string; }) =>

          <ListItem key={index.toString()}>
            <Tooltip title="Delete property filter">
            <IconButton
              aria-label="close"
              size="small"
              onClick={(e: any) => deleteProperty(e, index)}
              sx={{
                position: 'absolute',
                right: 0,
                top: 0
              }}
            >
              <CloseIcon />
              </IconButton>
            </Tooltip>
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
            </ListItem>
        )
      }
      </List>
    </Box>
  );
}

