import React from 'react';
import { useSelector } from "react-redux";
import { useEffect } from "react";
import { List, ListItem, ListItemText, Typography } from '@mui/material';

import * as IntegrationsStore from '../store/IntegrationsStates';
import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { IGetIntegrationLeafsDTO, IIntegrationLeafDTO } from "../store/Marker";

export function IntegrationLeafsViewer() {

  const appDispatch = useAppDispatch();
  const selected_integration: string | null | undefined =
    useSelector((state: ApplicationState) => state?.integrationsStates?.selected_integration);

  const integration_leafs: IGetIntegrationLeafsDTO | null =
    useSelector((state: ApplicationState) => state?.integrationsStates?.integration_leafs);

  const children: IIntegrationLeafDTO[] = integration_leafs?.children || [];

  useEffect(() => {
    if (selected_integration) {
      appDispatch(IntegrationsStore.fetchIntegrationLeafsByParent(selected_integration));
    }
  }, [selected_integration, appDispatch]);

  return (
    <div>
      <Typography variant="h6">Integration Leafs</Typography>
      <List>
        {children.length > 0 ? (
          children.map((child: IIntegrationLeafDTO) => (
            <ListItem key={child.id}>
              <ListItemText
                primary={child.id}
                secondary={child.parent_id ? `Parent ID: ${child.parent_id}` : 'No Parent ID'}
              />
            </ListItem>
          ))
        ) : (
          <Typography variant="body1">No children available</Typography>
        )}
      </List>
    </div>
  );
}
