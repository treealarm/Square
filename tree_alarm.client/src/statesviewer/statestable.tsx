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
import { ObjectStateDTO, ObjectStateDescriptionDTO } from '../store/Marker';
import { useAppDispatch } from '../store/configureStore';
import * as MarkersVisualStore from '../store/MarkersVisualStates';

interface Column {
  id: string;
  label: string;
  minWidth?: number;
  align?: 'right' | 'left';
  sortable?: boolean;
}

const columns: readonly Column[] = [
  { id: 'id', label: 'Object ID', minWidth: 150 },
  { id: 'states', label: 'States', minWidth: 200 },
  { id: 'alarm', label: 'Alarm', minWidth: 100 }
];

export default function StatesTable() {
  const appDispatch = useAppDispatch();

  const checked_ids = useSelector((state: ApplicationState) => state?.guiStates?.checked) ?? [];
  const visualStates = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates);
  const selectedState = useSelector((state: ApplicationState) => state?.markersVisualStates?.selected_state);

  const isAlarmed = (id: string) =>
    visualStates?.alarmed_objects.some((a) => a.id === id && a.alarm);

  const getDescr = (objId: string, state: string): ObjectStateDescriptionDTO | undefined =>
    visualStates?.states_descr.find((d) => d.id === objId && d.state === state);

  React.useEffect(() => {
    if (checked_ids.length > 0) {
      appDispatch(MarkersVisualStore.requestMarkersVisualStates(checked_ids));
    }
  }, [checked_ids]);

  const handleSelect = (obj: ObjectStateDTO) => {
    if (selectedState?.id === obj.id) {
      appDispatch(MarkersVisualStore.set_selected_state(null));
    } else {
      appDispatch(MarkersVisualStore.set_selected_state(obj));
    }
  };

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
                <TableCell key={column.id} align={column.align} style={{ minWidth: column.minWidth }}>
                  {column.label}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {visualStates?.states.map((obj: ObjectStateDTO) => {
              const selected = selectedState?.id === obj.id;
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

