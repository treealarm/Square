import * as React from 'react';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Button from '@mui/material/Button';
import { RichTreeView } from '@mui/x-tree-view/RichTreeView';
import { TreeViewBaseItem, TreeViewItemId } from '@mui/x-tree-view/models';

const MUI_X_PRODUCTS: TreeViewBaseItem[] = [
  {
    id: 'grid',
    label: 'Data Grid',
    children: [],
  },
  {
    id: 'pickers',
    label: 'Date and Time Pickers',
    children: [],
  },
  {
    id: 'charts',
    label: 'Charts',
    children: [],
  },
  {
    id: 'tree-view',
    label: 'Tree View',
    children: [{
      id: 'fake',
      label: 'fake'
    }],
  },
];



export function IntegrationViewer() {

  const [data, setData] = React.useState<TreeViewBaseItem[]>(MUI_X_PRODUCTS);

  const handleItemExpansionToggle = (
    event: React.SyntheticEvent,
    itemId: string,
    isExpanded: boolean,
  ) => {
    setData((prevData) => {
      return prevData.map((item) => {
        if (item.id === itemId) {
          if (isExpanded) {
            item.children = [];
            return {
              ...item,
              children: [
                { id: 'fake3', label: 'fake3' }, { id: 'fake4', label: 'fake4' },
                ...item.children
              ],
            };
          } else {
            item.children = [];
            return {
              ...item,
              children: [{ id: 'fake1', label: 'fake1' }],
            };
          }
        } else {
          return item;
        }
      });
    });
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
