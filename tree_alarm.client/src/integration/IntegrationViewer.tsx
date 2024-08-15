import * as React from 'react';
import { useSelector } from "react-redux";
import { useEffect, useState } from "react";

import {
  Box,
  Grid, Toolbar,
} from "@mui/material";

import { RichTreeView } from '@mui/x-tree-view/RichTreeView';
import { TreeViewBaseItem, TreeViewItemId } from '@mui/x-tree-view/models';

import * as IntegrationsStore from '../store/IntegrationsStates'
import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { IGetIntegrationsDTO, IIntegrationExDTO, uuidv4 } from "../store/Marker";
import { IntegrationToolbar } from './IntegrationToolbar';

function updateTreeData(tree: TreeViewBaseItem[], parentId: string, newChildren: TreeViewBaseItem[]): TreeViewBaseItem[] {
  return tree.map((node) => {
    if (node.id === parentId) {
      return { ...node, children: newChildren };
    }

    if (node.children) {
      return { ...node, children: updateTreeData(node.children, parentId, newChildren) };
    }

    return node;
  });
}

function getChildIds(tree: TreeViewBaseItem[], parentId: string): string[] {
  let ids: string[] = [];

  function findChildren(nodes: TreeViewBaseItem[]): void {
    for (const node of nodes) {
      if (node.id === parentId) {
        // Если узел с parentId найден, собираем идентификаторы всех его детей
        if (node.children) {
          // Рекурсивно собираем идентификаторы детей
          ids = node.children.flatMap(child => [child.id, ...getChildIds(node.children, child.id)]);
        }
        return; // Прекращаем дальнейший обход
      }
      // Если узел имеет дочерние элементы, продолжаем рекурсивный обход
      if (node.children) {
        findChildren(node.children);
      }
    }
  }

  findChildren(tree);
  return ids;
}

export function IntegrationViewer() {

  const [data, setData] = React.useState<TreeViewBaseItem[]>([]);
  const [expandedItems, setExpandedItems] = React.useState<string[]>([]);

  const appDispatch = useAppDispatch();
  const expanded_integration: IGetIntegrationsDTO | null | undefined =
    useSelector((state: ApplicationState) => state?.integrationsStates?.integrations);

  const transformChildren = (children: IIntegrationExDTO[]|null): TreeViewBaseItem[] => {    

    if(children == null)
    {
      return [];
    }
    return children.map(child => ({
      id: child.id,
      label: child.name,
      children: child.has_children ? [{ id: uuidv4(), label: uuidv4() }]:[]
    }));
  };

  useEffect(() => {
    appDispatch(IntegrationsStore.fetchIntegrationsByParent(''));
  }, []);

  useEffect(() => {
    
    if (expanded_integration) {
      const parentId = expanded_integration.parent_id || '';
      const newChildren = transformChildren(expanded_integration?.children);

    if(data.length == 0)
    {
      setData(newChildren);
      return;
    }
      setData((prevData) => updateTreeData(prevData, parentId, newChildren));
    }
  }, [expanded_integration]);

  const handleExpandedItemsChange = (
    event: React.SyntheticEvent,
    itemIds: string[],
  ) => {
    
  };

  
  const handleItemSelectionToggle = (
    event: React.SyntheticEvent,
    itemId: string,
    isSelected: boolean,
  ) => {
    if (isSelected) {
      appDispatch(IntegrationsStore.set_selected_integration(itemId));
    }
    else {

    }
  }

  const handleItemExpansionToggle = (
    event: React.SyntheticEvent,
    itemId: string,
    isExpanded: boolean,
  ) => {
    if (isExpanded) {
      appDispatch(IntegrationsStore.fetchIntegrationsByParent(itemId));
      
      setExpandedItems([...expandedItems, itemId]);
    }
    else{
      var children = getChildIds(data, itemId);
      var newExpandedItems =  expandedItems
      .filter(id => !children.includes(id) && itemId != id );
      setExpandedItems(newExpandedItems);
    }
  };

  return (
    <RichTreeView
      items={data}
      expandedItems={expandedItems}
      onItemExpansionToggle={handleItemExpansionToggle}
      onExpandedItemsChange={handleExpandedItemsChange}
      onItemSelectionToggle={handleItemSelectionToggle}
    />
  );
}
