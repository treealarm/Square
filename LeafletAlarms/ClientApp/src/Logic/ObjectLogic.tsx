import * as React from 'react';

import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as ObjLogicStore from '../store/ObjLogicStates';
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, IconButton, TextField } from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import SearchIcon from '@mui/icons-material/Search';
import AddIcon from '@mui/icons-material/Add';
import { ILogicFigureLinkDTO, IStaticLogicDTO } from '../store/Marker';
import { LogicEditor } from './LogicEditor';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectLogic() {

  const dispatch = useDispatch();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const logic = useSelector((state) => state?.objLogicStates?.logic);

  const [searchName, setSearchName] = React.useState<string>("");

  React.useEffect(() => {
   
  }, [selected_id]);

  const handleSave = useCallback(() => {
    if (logic != null && logic.length > 0) {
      dispatch(ObjLogicStore.actionCreators.updateObjLogic(logic));
    }    
  }, [logic]);

  const handleSearch = useCallback(() => {
    if (selected_id == null && searchName == "") {
      dispatch(ObjLogicStore.actionCreators.setObjLogicLocally([]));
    }
    else {
      if (searchName != "") {
        dispatch(ObjLogicStore.actionCreators.getObjLogicByName(searchName));
      }
      else {
        dispatch(ObjLogicStore.actionCreators.getObjLogic(selected_id));
      }      
    } 
  }, [selected_id, searchName]);

  function handleChangeSearchName(e: any) {
    const { target: { id, value } } = e;

    setSearchName(value);
  };

  const handleAdd = useCallback(() => {

      var newLogic: IStaticLogicDTO =
      {
        logic: 'new'
      }

      var copy: IStaticLogicDTO[] = [];

      if (logic == null) {
        copy = [newLogic];
      }
      else {
        copy = [...logic, newLogic];
      }

      dispatch(ObjLogicStore.actionCreators.setObjLogicLocally(copy));

  }, [logic]);

  const deleteFigureLink = useCallback(
    (logicObj: IStaticLogicDTO, item: ILogicFigureLinkDTO) => {

    let copy = Object.assign({}, logicObj);
    copy.figs = copy.figs.filter(i => i != item);

      var copyLogic = [...logic];
      var index2Replace = logic.findIndex(i => i == logicObj);
      copyLogic[index2Replace] = copy;

      dispatch(ObjLogicStore.actionCreators.setObjLogicLocally(copyLogic));
    }, [logic]);

  const deleteLogic = useCallback(
    (logicObj: IStaticLogicDTO) => {

      if (logicObj.id != null && logicObj.id != "") {
        dispatch(ObjLogicStore.actionCreators.delObjLogic(logicObj.id));
      }
      else {
        var newLogic = logic.filter(i => i != logicObj);
        dispatch(ObjLogicStore.actionCreators.setObjLogicLocally(newLogic));
      }      
    }, [logic]);

  const addFigureLink = useCallback(
    (logicObj: IStaticLogicDTO, item: ILogicFigureLinkDTO) => {

      let copy = Object.assign({}, logicObj);

      if (copy.figs == null) {
        copy.figs = [item];
      }
      else {
        copy.figs.push(item);
      }

      var copyLogic = [...logic];
      var index2Replace = logic.findIndex(i => i == logicObj);
      copyLogic[index2Replace] = copy;

      dispatch(ObjLogicStore.actionCreators.setObjLogicLocally(copyLogic));
    }, [logic]);

  const updateLogic = useCallback(
    (oldLogic: IStaticLogicDTO, newLogic: IStaticLogicDTO) => {

      var copyLogic = [...logic];
      var index2Replace = logic.findIndex(i => i == oldLogic);
      copyLogic[index2Replace] = newLogic;

      dispatch(ObjLogicStore.actionCreators.setObjLogicLocally(copyLogic));
    }, [logic]);

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
          <ButtonGroup variant="contained" aria-label="logic pannel">
            <IconButton aria-label="save" size="medium" onClick={handleSave}>
              <SaveIcon fontSize="inherit" />
            </IconButton>            

            <IconButton aria-label="add" size="medium" onClick={handleAdd}>
              <AddIcon fontSize="inherit" />
            </IconButton>

            <IconButton aria-label="search" size="medium" onClick={handleSearch}>
              <SearchIcon fontSize="inherit" />
            </IconButton>
          </ButtonGroup>

          <TextField size="small"
            fullWidth
            id={"searchByName"}
            label={"search by name"}
            value={searchName}
            onChange={handleChangeSearchName}
          />

        </ListItem>
        <ListItem>
          
        </ListItem>
        {
          logic?.map((item, index) =>
            <ListItem key={index}>
              <LogicEditor
                logicObj={item}
                deleteFigureLink={deleteFigureLink}
                addFigureLink={addFigureLink}
                deleteLogic={deleteLogic}
                updateLogic={updateLogic}
              />
            </ListItem>
          )
        }
      </List>
    </Box>
  );
}