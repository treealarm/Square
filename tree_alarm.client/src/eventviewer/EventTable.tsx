import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TableSortLabel from '@mui/material/TableSortLabel';
import { visuallyHidden } from '@mui/utils';

import { useAppDispatch } from '../store/configureStore';
import * as EventsStore from '../store/EventsStates'
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { DeepCopy, IEventDTO, LogLevel, SearchFilterDTO } from '../store/Marker';


interface Column {
  id: string;
  label: string;
  minWidth?: number;
  align?: 'right'|'left';
  // eslint-disable-next-line no-unused-vars
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
    id: 'event_priority',
    label: 'event_priority',
    minWidth: 170
  },
  
  {
    id: 'timestamp',
    label: 'timestamp',
    minWidth: 170,
    align: 'left',
  },
];

interface IEventTableProps {
  // eslint-disable-next-line no-unused-vars
  setLocalFilter: (newFilter: SearchFilterDTO) => any;
}
export default function EventTable(props: IEventTableProps) {

  const appDispatch = useAppDispatch();

  const events: IEventDTO[] = useSelector((state: ApplicationState) => state?.eventsStates?.events);
  const filter: SearchFilterDTO = useSelector((state: ApplicationState) => state?.eventsStates?.filter);
  const selected_event: IEventDTO = useSelector((state: ApplicationState) => state?.eventsStates?.selected_event);

  var order = filter.sort;


  const handleRequestSort = (id:string
  ) => {
    var newFilter = DeepCopy(filter);
    var curOrder = newFilter?.sort.find(el => el.key == id);

    if (curOrder == null) {
      newFilter.sort.push(
        {
          key: id,
          order: 'asc'
        });
    }
    else if (curOrder.order == 'asc') {
      curOrder.order = 'desc';
    }
    else if (curOrder.order == 'desc') {
      newFilter.sort = newFilter?.sort.filter(el => el.key != id);
    }

    props.setLocalFilter(newFilter);
    // fetch will be called from parent viewer in timeout
    //appDispatch(EventsStore.fetchEventsByFilter(newFilter));
  };

  const handleSelect = (row: IEventDTO
  ) => {
    if (selected_event?.id == row?.id) {
      appDispatch(EventsStore.set_selected_event(null));
    }
    else {
      appDispatch(EventsStore.set_selected_event(DeepCopy(row)));
    }
    
  };
  const getOrderByKey = (id:string) => {
    return order?.find(el => el.key == id);
  };

  const getColor = (event_priority: number) => {
    if (event_priority >= LogLevel.Error && event_priority <= LogLevel.Critical)
      return "rgba(" + (event_priority * 50).toString() + ",170,170,155)";
    if (event_priority >= LogLevel.Information && event_priority <= LogLevel.Warning)
      return "rgba(170,170," + (event_priority * 85).toString() + ",155)";

    return "rgba(170," + (event_priority * 50+200).toString() + ",170,155)";
  };

  const getPriority = (event_priority: number) => {

    if (event_priority in LogLevel)
      return LogLevel[event_priority];
    return event_priority.toString();
  }

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
        <Table stickyHeader aria-label="sticky table" size="small">
          <TableHead>
            <TableRow>
              {columns.map((column) => (

                <TableCell
                  key={column.id}
                  align={column.align}
                  style={{ minWidth: column.minWidth }}
                >
                  

                  <TableSortLabel
                    active={getOrderByKey(column.id) != null}
                    direction={getOrderByKey(column.id)?.order}
                    onClick={() => handleRequestSort(column.id)}
                  >
                    {column.label}
                    <Box component="span" sx={visuallyHidden}>
                      {getOrderByKey(column.id)?.order === 'desc' ? 'sorted descending' : 'sorted ascending'}
                      </Box>
                  </TableSortLabel>

                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {events?.map((row, index) => {
                return (
                  <TableRow
                    sx={{ backgroundColor: getColor(row.event_priority) }}
                    onClick={() => handleSelect(row)}
                    selected={selected_event != null && selected_event?.id == row?.id }
                    hover role="checkbox" tabIndex={-1} key={row.id + ' ' + index}>
                    {columns.map((column) => {
                      var value = null;
                      if (column.id == 'id') {
                        value = row.id;
                      }
                      if (column.id == 'object_id') {
                        value = row.object_id;
                      }
                      if (column.id == 'event_name') {
                        value = row.event_name;
                      }
                      if (column.id == 'event_priority') {
                        value = getPriority(row.event_priority);
                      }
                      
                      if (column.id == 'timestamp') {
                        var d = new Date(row.timestamp);
                        value = d.toLocaleString([],
                          {
                            year: 'numeric',
                            month: 'numeric',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit',
                            second: '2-digit'
                          }) + " [" + d.getMilliseconds() +"ms]";
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
    </Paper>
  );
}