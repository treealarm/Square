import * as React from 'react';

import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Autocomplete, Box, ButtonGroup, IconButton, TextField, ToggleButton } from '@mui/material';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import { ILogicFigureLinkDTO, IStaticLogicDTO } from '../store/Marker';
import CloseIcon from "@mui/icons-material/Close";
import DeleteIcon from '@mui/icons-material/Delete';
import LocationSearchingIcon from '@mui/icons-material/LocationSearching';
import { GroupSelector } from './GroupSelector';

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

  function replaceFigureLink(oldFig: ILogicFigureLinkDTO, newFig: ILogicFigureLinkDTO) {

    let copyLogic = Object.assign({}, logicObj);
    var index = copyLogic.figs.findIndex(i => i == oldFig);

    copyLogic.figs[index] = newFig;

    props.updateLogic(logicObj, copyLogic);
  }

  function handleChangeFigGroupId(id: number, newValue: string | null) {

     const fig = logicObj?.figs?.at(id);

    if (fig == null) {
      return;
    }
    let copy = Object.assign({}, fig);
    copy.group_id = newValue;

    replaceFigureLink(fig, copy);
  };

  function handleChangeFigId(e: any) {
    const { target: { id, value } } = e;

    const fig = logicObj?.figs?.at(id);

    if (fig == null) {
      return;
    }
    let copy = Object.assign({}, fig);
    copy.id = value;

    replaceFigureLink(fig, copy);
  };

  function handleChangeLogicType(id: any, value: any) {
    let copyLogic = Object.assign({}, logicObj);
    copyLogic.logic = value;

    props.updateLogic(logicObj, copyLogic);
  };

  function handleChangeLogicName(e: any) {
    const { target: { id, value } } = e;

    let copyLogic = Object.assign({}, logicObj);
    copyLogic.name = value;

    props.updateLogic(logicObj, copyLogic);
  };

  function UpdateFigureId() {
    if (figureIdMode != null &&
      selected_id != null &&
      selected_id != figureIdMode.id
    )
    {

      let copy = Object.assign({}, figureIdMode);
      copy.id = selected_id;

      replaceFigureLink(figureIdMode, copy);
    }
  }

  React.useEffect(() => {
    if (figureIdMode != null && selected_id != null) {

      UpdateFigureId();

      setFigureIdMode(null);
    }
  }, [selected_id]);

  React.useEffect(() => {
      UpdateFigureId();
  }, [figureIdMode]);

  const groups = [
    "from",
    "to"
  ];

  const logic_types = [
    "from-to",
    "counter"
  ];

  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List key="ListLogic">
        <ListItem key={"logic_id"}>

          <TextField size="small"
            fullWidth
            id={logicObj?.id} label={"logic_id"}
            value={logicObj?.id}
            disabled
          />
          <IconButton color="primary"
            aria-label="addProp"
            size="medium"
            onClick={(e) => deleteLogic(e)}          >

            <DeleteIcon fontSize="inherit"
              
            />

          </IconButton>
        </ListItem>

        <ListItem key={"logic_name"}>

          <TextField size="small"
            fullWidth
            id={logicObj?.name} label={"logic_name"}
            value={logicObj?.name}
            onChange={handleChangeLogicName}
          />

        </ListItem>

        <ListItem key={"logic_type"}>
          <GroupSelector
            id={"logic_type"}
            value={logicObj.logic}
            label={"logic type"}
            options={logic_types}
            onChange={handleChangeLogicType}
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

              
              <ListItem key={"FigGroupId_" + index}>
                <GroupSelector
                  id={index}
                  value={item.group_id} 
                  label={"group id"}
                  options={groups}
                  onChange={handleChangeFigGroupId}
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
                  id={index.toString()}
                  label={"fig_id"}
                  value={item.id}
                  onChange={handleChangeFigId}
                />

                <ToggleButton
                  color="success"
                  value="check"
                  aria-label="search"
                  selected={figureIdMode == item}
                  size="small"
                  onChange={
                    () => {
                      setFigureIdMode(figureIdMode == null ? item : null);                   
                    }                      
                  }>
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