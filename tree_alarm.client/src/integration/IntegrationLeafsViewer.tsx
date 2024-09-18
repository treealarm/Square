/* eslint-disable react-hooks/exhaustive-deps */
import { useSelector } from "react-redux";
import { useEffect } from "react";
import { List, ListItemText, Typography, Button, ListItemButton, Box, ListItem } from '@mui/material';

import * as IntegrationsStore from '../store/IntegrationsStates';
import * as GuiStore from '../store/GUIStates';
import * as ObjPropsStore from '../store/ObjPropsStates';

import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { IGetIntegrationLeafsDTO, IIntegrationLeafDTO, IObjProps } from "../store/Marker";

export function IntegrationLeafsViewer() {
  const appDispatch = useAppDispatch();
  const selected_integration: string | null | undefined =
    useSelector((state: ApplicationState) => state?.integrationsStates?.selected_integration);

  const integration_leafs: IGetIntegrationLeafsDTO | null =
    useSelector((state: ApplicationState) => state?.integrationsStates?.integration_leafs);

  const objProps: IObjProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const reduxSelectedId = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const children: IIntegrationLeafDTO[] = integration_leafs?.children || [];

  const handleAddLeaf = () => {
    if (selected_integration) {
      appDispatch(IntegrationsStore.addIntegrationLeaf(selected_integration))
        .then(() => {
          appDispatch(IntegrationsStore.fetchIntegrationLeafsByParent(selected_integration));
        });
    }
  };

  const handleDeleteLeaf = (id: string|null) => {
    if (selected_integration) {
      appDispatch(IntegrationsStore.deleteIntegrationLeaf(id))
        .then(() => {
          appDispatch(IntegrationsStore.fetchIntegrationLeafsByParent(selected_integration));
        });
    }
  };

  const handleSelectLeaf = (leaf: IIntegrationLeafDTO | null) => {
    appDispatch(GuiStore.selectTreeItem(leaf?.id ?? null));
  };

  useEffect(() => {
    if (selected_integration) {
      appDispatch(IntegrationsStore.fetchIntegrationLeafsByParent(selected_integration));
    }
  }, [selected_integration, appDispatch]);

  useEffect(() => {
    if (selected_integration) {
      appDispatch(IntegrationsStore.fetchIntegrationLeafsByParent(selected_integration));
    }
  }, [selected_integration, appDispatch]);

  useEffect(() => {
    var selected_child = children.findIndex(c => c.id == reduxSelectedId);
    if (selected_child < 0) {
      handleSelectLeaf(null);
    }
  }, [children, handleSelectLeaf, reduxSelectedId]);
  

  useEffect(() => {
    if (objProps == null) {

      var copy: IObjProps = {
        id: reduxSelectedId ?? "",
        name: 'New Object',
        extra_props:[]
      }
      appDispatch(ObjPropsStore.setObjPropsLocally(copy));
    }
  }, [objProps, reduxSelectedId, appDispatch]);

  return (
    <Box sx={{ display: 'flex', justifyContent: 'flex-start', marginBottom: 2 }}>
      
      <List sx={{ width: '100%' }}>
        <ListItem sx={{ width: '100%' }}>
          <Button
            onClick={handleAddLeaf}
            variant="contained"
            sx={{ marginLeft: 'auto', display: 'block' }}
          >
          Add Leaf
          </Button>
        </ListItem>
        
        {children.length > 0 ? (
          children.map((child: IIntegrationLeafDTO) => (
            <ListItemButton key={child.id} onClick={() => handleSelectLeaf(child)}
              selected={reduxSelectedId == child.id}
            >
              
              <ListItemText
                primary={child.id}
                secondary={child.parent_id ? `Parent ID: ${child.parent_id}` : 'No Parent ID'}
                
              />
              <Button onClick={() => handleDeleteLeaf(child?.id??null)} color="secondary">
                Delete
              </Button>
            </ListItemButton>
          ))
        ) : (
          <Typography variant="body1">No children available</Typography>
        )}
      </List>
    </Box>
  );
}
