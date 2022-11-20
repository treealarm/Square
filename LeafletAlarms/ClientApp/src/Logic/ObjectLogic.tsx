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

  React.useEffect(() => {
   
  }, [selected_id]);

  const handleSave = useCallback(() => {

  }, []);

  const handleSearch = useCallback(() => {
    if (selected_id == null) {
      dispatch(ObjLogicStore.actionCreators.setObjLogicLocally([]));
    }
    else {
      dispatch(ObjLogicStore.actionCreators.getObjLogic(selected_id));
    } 
  }, [selected_id]);

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
  },[logic]);


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
          <ButtonGroup variant="contained" aria-label="properties pannel">
            <IconButton aria-label="save" size="medium" onClick={handleSave}>
              <SaveIcon fontSize="inherit" />
            </IconButton>
            <IconButton aria-label="search" size="medium" onClick={handleSearch}>
              <SearchIcon fontSize="inherit" />
            </IconButton>

            <IconButton aria-label="add" size="medium" onClick={handleAdd}>
              <AddIcon fontSize="inherit" />
            </IconButton>
          </ButtonGroup>

        </ListItem> 
        {
          logic?.map((item, index) =>
            <ListItem key={index}>
              <LogicEditor logicObj={item} deleteFigureLink={deleteFigureLink} />
            </ListItem>
          )
        }
      </List>
    </Box>
  );
}