import * as React from 'react';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import DataObjectIcon from '@mui/icons-material/DataObject';
import SchemaIcon from '@mui/icons-material/Schema';
import LockPersonIcon from '@mui/icons-material/LockPerson';
import ManageSearchIcon from '@mui/icons-material/ManageSearch';
import SearchIcon from '@mui/icons-material/Search';
import SummarizeIcon from '@mui/icons-material/Summarize';
import { IPanelTypes } from '../store/Marker';

export function PanelIcon(props: { panelId: string }) {

  if (props.panelId == IPanelTypes.tree) {
    return (
      <AccountTreeIcon />
    );
  }

  if (props.panelId == IPanelTypes.search_result) {
    return (
      <ManageSearchIcon />
    );
  }

  if (props.panelId == IPanelTypes.properties) {
    return (
      <DataObjectIcon />
    );
  }

  if (props.panelId == IPanelTypes.search) {
    return (
      <SearchIcon />
    );
  }

  if (props.panelId == IPanelTypes.track_props) {
    return (
      <SummarizeIcon />
    );
  }

  if (props.panelId == IPanelTypes.rights) {
    return (
      <LockPersonIcon />
    );
  }
  return (
    <div />
  );
}