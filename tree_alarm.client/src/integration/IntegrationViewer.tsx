import * as React from 'react';
import { useSelector } from "react-redux";
import { useEffect, useState } from "react";

import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';

import { RichTreeView } from '@mui/x-tree-view/RichTreeView';
import { TreeViewBaseItem, TreeViewItemId } from '@mui/x-tree-view/models';

import * as IntegrationsStore from '../store/IntegrationsStates'
import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { IGetIntegrationsDTO, IIntegrationDTO } from './Marker';

const MUI_X_PRODUCTS: TreeViewBaseItem[] = [
  {
    id: '',
    label: 'Root',
    children: [{id: 'fake', label: 'fake'}],
  }
];



export function IntegrationViewer() {

  const [data, setData] = React.useState<TreeViewBaseItem[]>(MUI_X_PRODUCTS);

  const appDispatch = useAppDispatch();
  const expanded_integration: IGetIntegrationsDTO = useSelector((state: ApplicationState) => state?.integrationsStates?.integrations);

  const transformChildren = (children: IIntegrationDTO[]): TreeViewBaseItem[] => {    

    if(children == null)
    {
      return [];
    }
    return children.map(child => ({
      id: child.id,
      label: child.name,
      children: [] // ��� ������������� ����������, ���� ���� ��������� ����
    }));
  };

  useEffect(() => {
    appDispatch(IntegrationsStore.fetchIntegrationsByParent(''));
  }, []);

  useEffect(() => {
    
    if (expanded_integration != null) {
      setData((prevData) => {
        
        return prevData.map((item) => {
          console.log("item.id:", item.id, "expanded_integration:",expanded_integration);
          var parent_id = expanded_integration?.parent_id;
          if(parent_id == null)
          {
            parent_id = '';
          }
          if (item.id == parent_id) {
            return {
              ...item,
              children: transformChildren(expanded_integration.children),
            };
          } else {
            return item;
          }
        });
      });
    }
  }, [expanded_integration]);


  const handleItemExpansionToggle = (
    event: React.SyntheticEvent,
    itemId: string,
    isExpanded: boolean,
  ) => {
    if (isExpanded) {
      appDispatch(IntegrationsStore.fetchIntegrationsByParent(itemId));
    }    
  };

  return (
    <Stack spacing={2}>
      <Box sx={{ minHeight: 352, minWidth: 250 }}>
        <RichTreeView
          items={data}
          onItemExpansionToggle={handleItemExpansionToggle}
        />
      </Box>
    </Stack>
  );
}
