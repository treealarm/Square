/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Box, Divider, Grid, IconButton, Tooltip, Typography } from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import CategoryIcon from '@mui/icons-material/Category';

import { ApplicationState } from '../store';
import { useAppDispatch } from '../store/configureStore';
import * as GuiStore from '../store/GUIStates';
import * as DiagramsStore from '../store/DiagramsStates';
import { IDiagramFullDTO, IObjProps } from '../store/Marker';

import { DiagramEditingContext } from './DiagramEditingContext';
import { SearchableTreeBrowser } from '../tree/SearchableTreeBrowser';
import { DiagramComposition } from './DiagramComposition';
import { DiagramProperties, ChildEvents } from '../diagrams/DiagramProperties';
import DiagramNavigation from '../diagrams/DiagramNavigation';
import DiagramViewer from '../diagrams/DiagramViewer';
import { createDiagramReplicaOnDiagram } from '../diagrams/createDiagramReplica';

export function DiagramEditWorkspace() {
  const appDispatch = useAppDispatch();
  const navigate = useNavigate();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const objProps: IObjProps | null = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps ?? null);
  const cur_diagram_full: IDiagramFullDTO | null = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram ?? null);
  const cur_diagram_content = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content ?? null);
  const diagramDiving = useSelector((state: ApplicationState) => state?.guiStates?.diagramDiving);

  const cur_diagram = cur_diagram_full?.diagram ?? null;
  const [attachRefreshKey, setAttachRefreshKey] = useState(0);

  // The only "enter" affordance is selecting an object that has (or just got) a diagram —
  // exiting stays an explicit click on DiagramNavigation's existing Resurface button, so
  // browsing a diagram's own composition or search results doesn't unexpectedly kick you out.
  useEffect(() => {
    if (cur_diagram?.id && cur_diagram.id === selected_id) {
      appDispatch(DiagramsStore.fetchGetDiagramContent(cur_diagram.id));
      appDispatch(GuiStore.setDiagramDivingMode(true));
    }
  }, [cur_diagram?.id, selected_id]);

  const diagramPropEvents = useMemo<ChildEvents>(() => ({ clickSave: () => {} }), []);

  const handleSaveDiagram = () => {
    diagramPropEvents.clickSave();
  };

  const attachTargetDiagramId = diagramDiving ? cur_diagram_content?.diagram_id ?? null : null;

  const handleAttach = async (objectId: string) => {
    if (!attachTargetDiagramId) return;
    await createDiagramReplicaOnDiagram(appDispatch, objectId, attachTargetDiagramId, { left: 20, top: 20 }, cur_diagram_content);
    setAttachRefreshKey((k) => k + 1);
  };

  return (
    <DiagramEditingContext.Provider value={true}>
      <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', backgroundColor: '#bbbbbb', px: 1 }}>
          <Typography variant="subtitle1" sx={{ flexGrow: 1 }}>Diagram Editor</Typography>
          <Tooltip title="Diagram type editor">
            <IconButton onClick={() => navigate('/editdiagram')}>
              <CategoryIcon />
            </IconButton>
          </Tooltip>
        </Box>

        <Grid container sx={{ height: '100%', width: '100%', overflow: 'auto', flex: 1 }}>
          <Grid item xs={3} sx={{ height: '100%', border: 1 }}>
            <SearchableTreeBrowser
              onAttach={attachTargetDiagramId ? handleAttach : undefined}
              attachLabel="Attach to the open diagram"
            />
          </Grid>

          <Grid item xs sx={{ minWidth: '100px', minHeight: '100px', height: '100%', border: 1 }}>
            {diagramDiving ? (
              <DiagramViewer />
            ) : (
              <Box sx={{ p: 2 }}>
                <Typography color="text.secondary">
                  Select an object on the left. If it already has a diagram, it opens
                  automatically; if not, the &quot;Add Diagram for Object&quot; button on the
                  right creates one.
                </Typography>
              </Box>
            )}
          </Grid>

          <Grid item xs={3} sx={{ height: '100%', border: 1, overflow: 'auto' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', px: 1 }}>
              <Typography variant="subtitle2" sx={{ flexGrow: 1 }}>{objProps?.name ?? ''}</Typography>
              {diagramDiving && (
                <Tooltip title="Save">
                  <IconButton size="small" onClick={handleSaveDiagram}>
                    <SaveIcon fontSize="inherit" />
                  </IconButton>
                </Tooltip>
              )}
            </Box>
            <Divider />
            {diagramDiving && <DiagramNavigation />}
            <DiagramProperties events={diagramPropEvents} />
            {attachTargetDiagramId && (
              <>
                <Divider />
                <DiagramComposition diagramId={attachTargetDiagramId} refreshKey={attachRefreshKey} />
              </>
            )}
          </Grid>
        </Grid>
      </Box>
    </DiagramEditingContext.Provider>
  );
}
