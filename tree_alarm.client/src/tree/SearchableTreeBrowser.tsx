import * as React from 'react';
import { useCallback, useRef, useState } from 'react';
import {
  Box, TextField, List, ListItem, ListItemButton, ListItemText,
  IconButton, Tooltip, CircularProgress, Typography
} from '@mui/material';
import AttachFileIcon from '@mui/icons-material/AttachFile';
import { TreeControl } from './TreeControl';
import * as TreeStore from '../store/TreeStates';
import * as GuiStore from '../store/GUIStates';
import { useAppDispatch } from '../store/configureStore';
import { Marker } from '../store/Marker';

export interface ISearchableTreeBrowserProps {
  // When provided, each search result gets an extra "attach" action (e.g. attach to the
  // diagram currently being edited) — the actual attach logic lives with the caller, this
  // component only surfaces the action and reports which object it was clicked for.
  // eslint-disable-next-line no-unused-vars
  onAttach?: (objectId: string) => void;
  attachLabel?: string;
}

const SEARCH_DEBOUNCE_MS = 400;

export function SearchableTreeBrowser({ onAttach, attachLabel }: ISearchableTreeBrowserProps) {
  const appDispatch = useAppDispatch();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<Marker[]>([]);
  const [loading, setLoading] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const runSearch = useCallback((q: string) => {
    if (!q.trim()) {
      setResults([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    TreeStore.searchByName(q)
      .then((found) => setResults(found))
      .catch(() => setResults([]))
      .finally(() => setLoading(false));
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setQuery(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => runSearch(value), SEARCH_DEBOUNCE_MS);
  };

  // Jump to where the found object lives — load its parent's level (which also gives us the
  // breadcrumb via GetByParent's `parents`) and select it, instead of manually drilling down.
  const goTo = (item: Marker) => {
    appDispatch(TreeStore.setParentIdLocally({ parent_id: item.parent_id ?? null, start_id: null, end_id: null }));
    appDispatch(GuiStore.selectTreeItem(item.id));
    setResults([]);
    setQuery('');
  };

  return (
    <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ p: 1 }}>
        <TextField
          size="small"
          fullWidth
          placeholder="Search object by name..."
          value={query}
          onChange={handleChange}
        />
      </Box>
      {(loading || results.length > 0) && (
        <Box sx={{ maxHeight: 260, overflow: 'auto', borderBottom: 1, borderColor: 'divider' }}>
          {loading && (
            <Box sx={{ p: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
              <CircularProgress size={16} />
              <Typography variant="body2">Searching...</Typography>
            </Box>
          )}
          {!loading && results.length === 0 && query.trim() && (
            <Typography variant="body2" sx={{ p: 1 }} color="text.secondary">No results</Typography>
          )}
          <List dense>
            {results.map((item) => (
              <ListItem
                key={item.id}
                disablePadding
                secondaryAction={onAttach && (
                  <Tooltip title={attachLabel ?? 'Attach'}>
                    <IconButton size="small" edge="end" onClick={() => item.id && onAttach(item.id)}>
                      <AttachFileIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                )}
              >
                <ListItemButton onClick={() => goTo(item)}>
                  <ListItemText primary={item.name} />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        </Box>
      )}
      <Box sx={{ flex: 1, minHeight: 0 }}>
        <TreeControl />
      </Box>
    </Box>
  );
}
