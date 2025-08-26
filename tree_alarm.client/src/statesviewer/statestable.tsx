/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import * as React from 'react';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Chip from '@mui/material/Chip';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import { IObjectStateDTO, Marker, ObjectStateDescriptionDTO } from '../store/Marker';
import { useAppDispatch } from '../store/configureStore';
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import * as GuiStore from '../store/GUIStates';
import { fetchSimpleMarkersByIds } from '../store/MarkersStates';
import { useState } from 'react';

interface Column {
  id: string;
  label: string;
  minWidth?: number;
  align?: 'right' | 'left';
  sortable?: boolean;
}

const columns: readonly Column[] = [
  { id: 'id', label: 'Object ID', minWidth: 100 },
  { id: 'name', label: 'Name', minWidth: 200 }, 
  { id: 'states', label: 'States', minWidth: 200 },
  { id: 'alarm', label: 'Alarm', minWidth: 100 }
];

export default function StatesTable() {
  const appDispatch = useAppDispatch();

  const checked_ids = useSelector((state: ApplicationState) => state?.guiStates?.checked) ?? [];
  const visualStates = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates);
  const selectedStateId = useSelector((state: ApplicationState) => state?.markersVisualStates?.selected_state_id);

  const isAlarmed = (id: string) =>
    visualStates?.alarmed_objects.some((a) => a.id === id && a.alarm);

  const getDescr = (objId: string, state: string): ObjectStateDescriptionDTO | undefined =>
    visualStates?.states_descr.find((d) => d.id === objId && d.state === state);

  const [markers, setMarkers] = useState<Marker[]>([]);

  const [sortConfig, setSortConfig] = React.useState<{ key: string; direction: 'asc' | 'desc' } | null>(null);

  const handleSort = (key: string) => {
    setSortConfig(prev => {
      if (prev && prev.key === key) {
        // меняем направление, если сортируем по той же колонке
        return { key, direction: prev.direction === 'asc' ? 'desc' : 'asc' };
      }
      return { key, direction: 'asc' }; // новая колонка — сортируем по возрастанию
    });
  };

  const loadMarkers = async (ids: string[]) => {
    const resultAction = await appDispatch(fetchSimpleMarkersByIds(ids));

    if (fetchSimpleMarkersByIds.fulfilled.match(resultAction)) {
      setMarkers(resultAction.payload); // сохраняем только локально
    }
  };

  React.useEffect(() => {
    // Redux — состояния объектов
    appDispatch(MarkersVisualStore.requestMarkersVisualStates(checked_ids));

    if (checked_ids.length > 0) { 
      // Локально — имена маркеров
      loadMarkers(checked_ids);
    } else {
      // Если ids пустые — очищаем локальные имена
      setMarkers([]);
    }
  }, [checked_ids]);

  const handleSelect = (obj: IObjectStateDTO) => {
    if (selectedStateId === obj.id) {
      appDispatch(GuiStore.selectTreeItem(null));
      appDispatch(MarkersVisualStore.set_selected_state(null));
    } else {
      appDispatch(GuiStore.selectTreeItem(obj.id));
      appDispatch(MarkersVisualStore.set_selected_state(obj.id));
    }
  };

  let sortedStates = visualStates?.states ?? [];

  if (sortConfig) {
    sortedStates = [...sortedStates].sort((a, b) => {
      const markerA = markers.find(m => m.id === a.id);
      const markerB = markers.find(m => m.id === b.id);

      let valueA: string | number = '';
      let valueB: string | number = '';

      if (sortConfig.key === 'id') {
        valueA = a.id;
        valueB = b.id;
      } else if (sortConfig.key === 'name') {
        valueA = markerA?.name || '';
        valueB = markerB?.name || '';
      } else if (sortConfig.key === 'alarm') {
        // true / false для тревоги
        valueA = isAlarmed(a.id) ? 1 : 0;
        valueB = isAlarmed(b.id) ? 1 : 0;
      }
      else if (sortConfig.key === 'states') {
        const statesA = [...a.states].sort().join(','); // сортируем строки и объединяем
        const statesB = [...b.states].sort().join(',');

        valueA = statesA;
        valueB = statesB;
      }
      if (valueA < valueB) return sortConfig.direction === 'asc' ? -1 : 1;
      if (valueA > valueB) return sortConfig.direction === 'asc' ? 1 : -1;
      return 0;
    });
  }

  return (
    <Paper
      sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        border: 0,
        padding: 0,
        margin: 0,
        position: 'relative'
      }}
    >
      <TableContainer>
        <Table stickyHeader size="small">
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell
                  key={column.id}
                  align={column.align}
                  style={{ minWidth: column.minWidth, cursor: 'pointer' }}
                  onClick={() => handleSort(column.id)}
                >
                  {column.label}
                  {sortConfig?.key === column.id ? (sortConfig.direction === 'asc' ? ' 🔼' : ' 🔽') : ''}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>

          <TableBody>
            {sortedStates.map((obj: IObjectStateDTO) => {
              const selected = selectedStateId == obj.id;
              const marker = markers.find(m => m.id === obj.id);

              return (
                <TableRow
                  key={obj.id}
                  hover
                  onClick={() => handleSelect(obj)}
                  selected={selected}
                  sx={{
                    cursor: 'pointer',
                    backgroundColor: selected
                      ? '#c8e6c9'
                      : isAlarmed(obj.id)
                        ? '#ffebee'
                        : 'inherit'
                  }}
                >
                  <TableCell>{obj.id}</TableCell>
                  <TableCell>{marker?.name ?? ''}</TableCell>
                  <TableCell>
                    {obj.states.map((s) => {
                      const descr = getDescr(obj.id, s);
                      return (
                        <Chip
                          key={s}
                          label={descr ? descr.state_descr : s}
                          size="small"
                          sx={{
                            mr: 0.5,
                            backgroundColor: descr?.state_color || 'grey.400',
                            color: '#fff'
                          }}
                        />
                      );
                    })}
                  </TableCell>
                  <TableCell>{isAlarmed(obj.id) ? '🚨' : ''}</TableCell>
                </TableRow>
              );
            })}
          </TableBody>

        </Table>
      </TableContainer>
    </Paper>
  );
}

