/* eslint-disable react-hooks/exhaustive-deps */
import ScubaDivingIcon from '@mui/icons-material/ScubaDiving';
import PoolIcon from '@mui/icons-material/Pool';

import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { IDiagramDTO, IDiagramFullDTO } from '../store/Marker';

import * as DiagramsStore from '../store/DiagramsStates';
import * as GUIStore from '../store/GUIStates';
import { useAppDispatch } from '../store/configureStore';
import { useCallback, useEffect } from 'react';
import { Box, IconButton, Tooltip } from '@mui/material';
import DiagramParentsNavigator from './DiagramParentsNavigator';

export default function DiagramNavigation() {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram_content = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content);
  const cur_diagram_full: IDiagramFullDTO | null = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram ?? null);  
  const diagramDiving = useSelector((state: ApplicationState) => state?.guiStates?.diagramDiving);

  const cur_diagram: IDiagramDTO | null = cur_diagram_full?.diagram ?? null;

  const appDispatch = useAppDispatch();

  useEffect(() => {
  }, []);

  //useEffect(() => {

  //  if (!cur_diagram) {
  //    Resurface();
  //  }
  //}, [cur_diagram]);

  const setDiagram = useCallback(
    (diagram_id: string|null) => {
      if (diagram_id == null) {
        return;
      }
      appDispatch(DiagramsStore.fetchGetDiagramContent(diagram_id));
      appDispatch(GUIStore.setDiagramDivingMode(true));
    }, [ ]);

  const Resurface = useCallback(
    () => {
      //appDispatch<any>(DiagramsStore.update_single_diagram_locally(null));
      appDispatch(GUIStore.setDiagramDivingMode(false));
    }, [ ]);

  if (!cur_diagram && !diagramDiving) {
    return null;
  }

  return (
    <Box
      sx={{
        backgroundColor: '#bbbbbb',
        flexDirection: 'row',
        justifyContent: 'flex-end',
        display: 'flex'
      }}
    >
      {diagramDiving ?
        <Tooltip title={"Resurface from the diagram"}>
          <IconButton aria-label="search" size="medium" onClick={() => Resurface()}>
            <PoolIcon fontSize="inherit" />
          </IconButton>
        </Tooltip>:<div></div>
      }

      <DiagramParentsNavigator parent_list={diagram_content?.parents ?? null} parent_id={cur_diagram?.id ?? null} />

      <Tooltip title={"Dive into the diagram"}>
        <IconButton aria-label="search" size="medium" onClick={() => setDiagram(objProps?.id ??null)}>
          <ScubaDivingIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>
    </Box>
  );
}