import * as React from 'react';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TablePagination from '@mui/material/TablePagination';
import TableRow from '@mui/material/TableRow';
import TableSortLabel from '@mui/material/TableSortLabel';
import { visuallyHidden } from '@mui/utils';

import { useAppDispatch } from '../store/configureStore';
import * as EventsStore from '../store/EventsStates'
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { DeepCopy, IEventDTO, SearchFilterDTO } from '../store/Marker';


interface Column {
  id: string;
  label: string;
  minWidth?: number;
  align?: 'right'|'left';
  format?: (value: number) => string;
}

const columns: readonly Column[] = [
  { id: 'id', label: 'id', minWidth: 170 },
  { id: 'object_id', label: 'object_id', minWidth: 100 },
  {
    id: 'event_name',
    label: 'event_name',
    minWidth: 170,
    align: 'left',
    format: (value: number) => value.toLocaleString('en-US'),
  },  
  {
    id: 'timestamp',
    label: 'timestamp',
    minWidth: 170,
    align: 'left',
  },
];


export default function EventViewer() {

  const appDispatch = useAppDispatch();
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);

  const events: IEventDTO[] = useSelector((state: ApplicationState) => state?.eventsStates?.events);
  const filter: SearchFilterDTO = useSelector((state: ApplicationState) => state?.eventsStates?.filter);

  var order = filter.sort;

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(+event.target.value);
    setPage(0);
  };

  const handleRequestSort = (id:string
  ) => {
    var newFilter = DeepCopy(filter);

    if (order[id] == undefined) {
      newFilter.sort[id] = 'asc';
    }
    if (order[id] == 'asc') {
      newFilter.sort[id] = 'desc';
    }
    if (order[id] == 'desc') {
      delete newFilter.sort[id];
    }

    appDispatch(EventsStore.set_local_filter(newFilter));
    appDispatch(EventsStore.fetchEventsByFilter(newFilter));
  };

  return (
    <Paper sx={{
      width: '100%',
      height: '100%',
      display: 'flex',
      flexDirection: 'column',
      border: 0,
      padding: 0,
      margin: 0,
      position: 'relative',
    }}>
      <TableContainer >
        <Table stickyHeader aria-label="sticky table">
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell
                  key={column.id}
                  align={column.align}
                  style={{ minWidth: column.minWidth }}
                >
                  

                  <TableSortLabel
                    active={order[column.id] != undefined}
                    direction={order[column.id]}
                    onClick={() => handleRequestSort(column.id)}
                  >
                    {column.label}
                    <Box component="span" sx={visuallyHidden}>
                      {order[column.id] === 'desc' ? 'sorted descending' : 'sorted ascending'}
                      </Box>
                  </TableSortLabel>

                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {events?.map((row) => {
                return (
                  <TableRow hover role="checkbox" tabIndex={-1} key={row.meta.id}>
                    {columns.map((column) => {
                      var value = null;
                      if (column.id == 'id') {
                        value = row.meta.id;
                      }
                      if (column.id == 'object_id') {
                        value = row.meta.object_id;
                      }
                      if (column.id == 'event_name') {
                        value = row.meta.event_name;
                      }
                      if (column.id == 'timestamp') {
                        value = new Date(row.timestamp).toLocaleDateString();
                      }
 
                      return (
                        <TableCell key={column.id} align={column.align}>
                          {column.format && typeof value === 'number'
                            ? column.format(value)
                            : value}
                        </TableCell>
                      );
                    })}
                  </TableRow>
                );
              })}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        rowsPerPageOptions={[10, 25, 100]}
        component="div"
        count={events?.length}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handleChangePage}
        onRowsPerPageChange={handleChangeRowsPerPage}
        sx={{
          display: 'flex',
          minHeight: '70px',
      } }
        
      />
    </Paper>
  );
}