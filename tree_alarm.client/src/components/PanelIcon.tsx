import React from 'react';

import AccountTreeIcon from '@mui/icons-material/AccountTree';
import DataObjectIcon from '@mui/icons-material/DataObject';
import LockPersonIcon from '@mui/icons-material/LockPerson';
import ManageSearchIcon from '@mui/icons-material/ManageSearch';
import SearchIcon from '@mui/icons-material/Search';
import SummarizeIcon from '@mui/icons-material/Summarize';
import SportsEsportsIcon from '@mui/icons-material/SportsEsports';
import { IPanelTypes } from '../store/Marker';

const panelIcons: Record<string, React.ReactElement> = {
  [IPanelTypes.tree]: <AccountTreeIcon />,
  [IPanelTypes.search_result]: <ManageSearchIcon />,
  [IPanelTypes.properties]: <DataObjectIcon />,
  [IPanelTypes.search]: <SearchIcon />,
  [IPanelTypes.track_props]: <SummarizeIcon />,
  [IPanelTypes.rights]: <LockPersonIcon />,
  [IPanelTypes.actions]: <SportsEsportsIcon />,
};

export function PanelIcon({ panelId }: { panelId: string }) {
  return panelIcons[panelId] || <div />;
}
