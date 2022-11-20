import * as React from 'react';

import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, IconButton, TextField, ToggleButton } from '@mui/material';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import { ILogicFigureLinkDTO, IStaticLogicDTO } from '../store/Marker';
import CloseIcon from "@mui/icons-material/Close";
import DeleteIcon from '@mui/icons-material/Delete';
import LocationSearchingIcon from '@mui/icons-material/LocationSearching';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function LogicEditor(props: any) {

  const logicObj: IStaticLogicDTO = props.logicObj;
  const dispatch = useDispatch();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const logic = useSelector((state) => state?.objLogicStates?.logic);

  const [figureIdMode, setFigureIdMode] = React.useState <ILogicFigureLinkDTO>(null);

  function deleteFigureLink
    (e: any, item: ILogicFigureLinkDTO) {
    props.deleteFigureLink(logicObj, item);
  };

  function deleteLogic
    (e: any) {
    props.deleteLogic(logicObj);
  };

  function addFigureLink
    (e: any) {
    const item: ILogicFigureLinkDTO = {
      group_id: "new_group",
      id: ""
    };
    props.addFigureLink(logicObj, item);
  };


  React.useEffect(() => {
    if (figureIdMode != null && selected_id != null) {

      let copy = Object.assign({}, figureIdMode);
      copy.id = selected_id;

      let copyLogic = Object.assign({}, logicObj);
      var index = copyLogic.figs.findIndex(i => i == figureIdMode);

      copyLogic.figs[index] = copy;

      props.updateLogic(logicObj, copyLogic);

      setFigureIdMode(null);
    }
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
        <ListItem key={logicObj.id}>

          <TextField size="small"
            fullWidth
            id={logicObj?.id} label={"logic_id"}
            value={logicObj?.id}
            disabled
          />
          <IconButton color="primary"
            aria-label="addProp"
            size="medium">

            <DeleteIcon fontSize="inherit"
              onClick={(e) => deleteLogic(e)}
            />

          </IconButton>
        </ListItem>

        <ListItem key={logicObj.id}>

          <TextField size="small"
            fullWidth
            id={logicObj?.logic} label={"logic_name"}
            value={logicObj?.logic}
          />

        </ListItem>
        
        <ButtonGroup variant="contained" aria-label="logic pannel">

          <IconButton color="primary"
            aria-label="addProp"
            size="medium"
            onClick={(e) => addFigureLink(e)}>
            <LibraryAddIcon fontSize="inherit" />
          </IconButton>

          

        </ButtonGroup>

        {
          logicObj?.figs?.map((item, index) =>
            <List sx={{
              border: 1
            }}>


              <ListItem key={index}>
                <TextField size="small"
                  fullWidth
                  id={item.group_id} label={"group_id"}
                  value={item.group_id}
                />

                <IconButton
                  aria-label="close"
                  size="small"
                  onClick={(e) => deleteFigureLink(e, item)}
                >
                  <CloseIcon />
                </IconButton>
              </ListItem>

              <ListItem key={index}>

                <TextField size="small"
                  fullWidth
                  id={item.id} label={"fig_id"}
                  value={item.id}
                />

                <ToggleButton
                  color="success"
                  value="check"
                  aria-label="search"
                  selected={figureIdMode == item}
                  size="small"
                  onChange={() => setFigureIdMode(figureIdMode == null ? item:null)}>
                  <LocationSearchingIcon fontSize="small" />
                </ToggleButton>
                
              </ListItem>
            </List>
          )
        }
      </List>
    </Box>
  );
}