﻿import * as React from 'react';

import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, IconButton, TextField, ToggleButton, Tooltip } from '@mui/material';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import { DeepCopy, ILogicFigureLinkDTO, IStaticLogicDTO, ObjPropsSearchDTO } from '../store/Marker';
import CloseIcon from "@mui/icons-material/Close";
import DeleteIcon from '@mui/icons-material/Delete';
import LocationSearchingIcon from '@mui/icons-material/LocationSearching';
import { GroupSelector } from './GroupSelector';
import { PropertyFilter } from '../tree/PropertyFilter';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function LogicEditor(props: any) {

  const logicObj: IStaticLogicDTO = props.logicObj;
  const dispatch = useDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const logic = useSelector((state: ApplicationState) => state?.objLogicStates?.logic);

  const [figureIdMode, setFigureIdMode] = React.useState<ILogicFigureLinkDTO>(null);

  const [groupArray, setGroupArray] = React.useState([]);

  function deleteFigureLink
    (e: any, item: number) {
    props.deleteFigureLink(logicObj, item);
  };

  function deleteLogic
    (e: any) {
    props.deleteLogic(logicObj);
  };

  function addFigureLink
    (e: any) {
    const item: ILogicFigureLinkDTO = {
      group_id: "",
      id: ""
    };
    props.addFigureLink(logicObj, item);
  };


  function replaceFigureLink(oldFig: ILogicFigureLinkDTO, newFig: ILogicFigureLinkDTO) {

    let copyLogic = DeepCopy(logicObj);
    //let copyLogic: IStaticLogicDTO = (JSON.parse(JSON.stringify(logicObj)));

    var index = logicObj.figs.findIndex(i => i == oldFig);

    copyLogic.figs[index] = newFig;

    props.updateLogic(logicObj, copyLogic);
  }

  function handleChangeFigGroupId(id: number, newValue: string | null) {

     const fig = logicObj?.figs?.at(id);

    if (fig == null) {
      return;
    }
    let copy = DeepCopy(fig);
    copy.group_id = newValue;

    replaceFigureLink(fig, copy);
  };

  function handleChangeFigId(e: any) {
    const { target: { id, value } } = e;

    const fig = logicObj?.figs?.at(id);

    if (fig == null) {
      return;
    }
    let copy = DeepCopy(fig);
    copy.id = value;

    replaceFigureLink(fig, copy);
  };

  function handleChangeLogicType(id: any, value: any) {
    let copyLogic = DeepCopy(logicObj);
    copyLogic.logic = value;

    props.updateLogic(logicObj, copyLogic);
  };

  function handleChangeLogicName(e: any) {
    const { target: { id, value } } = e;

    let copyLogic = DeepCopy(logicObj);
    copyLogic.name = value;

    props.updateLogic(logicObj, copyLogic);
  };

  function UpdateFigureId() {
    if (figureIdMode != null &&
      selected_id != null &&
      selected_id != figureIdMode.id
    )
    {

      let copy = DeepCopy(figureIdMode);
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

  const setPropsFilter = useCallback(
    (propsFilter: ObjPropsSearchDTO) => {

      let copyLogic = DeepCopy(logicObj);
      copyLogic.property_filter = propsFilter;
      props.updateLogic(logicObj, copyLogic);
    }, [logicObj]);

  const addProperty = useCallback(
    (e: any) => {
      let copyLogic = DeepCopy(logicObj);
      if (copyLogic.property_filter == null) {
        copyLogic.property_filter = {
          props: []
        };
      }
      copyLogic.property_filter.props.push({ prop_name: "test_name", str_val: "test_val" });
      props.updateLogic(logicObj, copyLogic);
    }, [logicObj]);

  
  React.useEffect(() => {
    if (logicObj?.logic == "from-to") {
      setGroupArray(["from", "to", "gr_text"]);
    }
    else {
      setGroupArray(["count", "gr_text"]);
    }

  }, [logicObj.logic]);

  const logic_types = [
    "from-to",
    "counter"
  ];

  return (
    <Box sx={{
      width: '100%',
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
          <Tooltip title="Delete this logic">

          <IconButton
            aria-label="addProp"
            size="medium"
            onClick={(e: any) => deleteLogic(e)}          >

            <DeleteIcon fontSize="inherit"
              
            />

            </IconButton>
          </Tooltip>
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

          <IconButton
            aria-label="addProp"
            size="medium"
            onClick={(e: any) => addFigureLink(e)}>
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
                  options={groupArray}
                  onChange={handleChangeFigGroupId}
                />
                <Tooltip title="Delete this group">
                <IconButton
                  aria-label="close"
                  size="small"
                    onClick={(e: any) => deleteFigureLink(e, index)}
                >
                  <CloseIcon />
                  </IconButton>
                </Tooltip>
              </ListItem>

              <ListItem key={index}>

                <TextField size="small"
                  fullWidth
                  id={index.toString()}
                  label={"fig_id"}
                  value={item.id}
                  onChange={handleChangeFigId}
                />
                <Tooltip title="Find figure of the logic">
                <ToggleButton
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
                </Tooltip>
              </ListItem>
            </List>
          )
        }
        <Box display="flex"
          justifyContent="flex-start"
        >
          <Tooltip title="Add property filter">
          <IconButton aria-label="addProp" size="medium" onClick={(e: any) => addProperty(e)}>
            <LibraryAddIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>
        </Box>

        <ListItem>
          
          <PropertyFilter
            propsFilter={logicObj?.property_filter}
            setPropsFilter={setPropsFilter} />
        </ListItem>
      </List>
    </Box>
  );
}