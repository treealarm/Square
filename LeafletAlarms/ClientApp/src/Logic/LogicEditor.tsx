import * as React from 'react';

import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as ObjLogicStore from '../store/ObjLogicStates';
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, IconButton, Stack, TextField } from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import SearchIcon from '@mui/icons-material/Search';
import AddIcon from '@mui/icons-material/Add';
import { ILogicFigureLinkDTO, IStaticLogicDTO } from '../store/Marker';
import CloseIcon from "@mui/icons-material/Close";

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function LogicEditor(props: any) {

  const logicObj: IStaticLogicDTO = props.logicObj;
  const dispatch = useDispatch();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const logic = useSelector((state) => state?.objLogicStates?.logic);

  function deleteProperty
    (e: any, item: ILogicFigureLinkDTO) {
    props.deleteFigureLink(logicObj, item);
  };

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
            id={logicObj?.logic} label={"logic_name"}
            value={logicObj?.logic}
          />

        </ListItem>
        <ListItem key={logicObj.id}>

          <TextField size="small"
            fullWidth
            id={logicObj?.id} label={"logic_id"}
            value={logicObj?.id}
          />

        </ListItem>

        {
          logicObj?.figs.map((item, index) =>
            <List sx={{
              border: 1
            }}>


              <ListItem key={index}>

                <TextField size="small"
                  fullWidth
                  id={item.id} label={"fig_id"}
                  value={item.id}
                />
                <IconButton
                  aria-label="close"
                  size="small"
                  onClick={(e) => deleteProperty(e, item)}
                >
                  <CloseIcon />
                </IconButton>
              </ListItem>

              <ListItem key={index}>
                <TextField size="small"
                  fullWidth
                  id={item.group_id} label={"group_id"}
                  value={item.group_id}
                />
              </ListItem>
            </List>
          )
        }
      </List>
    </Box>
  );
}