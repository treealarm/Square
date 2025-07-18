/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */
import * as React from 'react';

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
import { DeepCopy, IEventDTO, LogLevel, SearchEventFilterDTO } from '../store/Marker';


interface Column {
  id: string;
  label: string;
  minWidth?: number;
  align?: 'right' | 'left';
  sortable?: boolean;
  // eslint-disable-next-line no-unused-vars
  format?: (value: number) => string;
}

const columns: readonly Column[] = [
  {
    id: 'id',
    label: 'id',
    minWidth: 170,
    align: 'left',
    sortable: false
  },
  {
    id: 'object_id',
    label: 'object_id',
    minWidth: 100
  },
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
  {
    id: 'param0',
    label: 'param0',
    minWidth: 170,
    align: 'left',
    sortable: false
  },
  {
    id: 'param1',
    label: 'param1',
    minWidth: 170,
    align: 'left',
    sortable: false
  },
];

interface IEventTableProps {
  // eslint-disable-next-line no-unused-vars
  setLocalFilter: (newFilter: SearchEventFilterDTO|null) => any;
  onSelect: (event: IEventDTO | null) => void;
}
export default function EventTable(props: IEventTableProps) {

  const appDispatch = useAppDispatch();

  const events: IEventDTO[] = useSelector((state: ApplicationState) => state?.eventsStates?.events) ?? [];
  const filter: SearchEventFilterDTO|null = useSelector((state: ApplicationState) => state?.eventsStates?.filter) ?? null;
  const selected_event: IEventDTO|null = useSelector((state: ApplicationState) => state?.eventsStates?.selected_event) ?? null;

  var order = filter?.sort??[];


  const handleRequestSort = (id: string) => {
    const newFilter = DeepCopy(filter) ?? { sort: [] };
    const sortList = newFilter.sort ?? [];

    // Удаляем старую запись, если она уже есть
    const index = sortList.findIndex(s => s.key === id);
    if (index !== -1) {
      const existing = sortList[index];
      existing.order = existing.order === 'asc' ? 'desc' : 'asc';
      sortList.splice(index, 1); // удаляем
      sortList.push(existing);   // добавляем в конец (выше будет reverse)
    } else {
      sortList.push({ key: id, order: 'asc' });
    }

    // Гарантируем, что timestamp всегда на первом месте
    const timestampIndex = sortList.findIndex(s => s.key === 'timestamp');
    if (timestampIndex !== -1) {
      const ts = sortList.splice(timestampIndex, 1)[0];
      sortList.unshift(ts);
    } else {
      // Если вдруг нет — добавим
      sortList.unshift({ key: 'timestamp', order: 'desc' });
    }

    newFilter.sort = sortList;
    props.setLocalFilter(newFilter);
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
                  

                  {column.sortable !== false ? (
                    <TableSortLabel
                      active={getOrderByKey(column.id) != null}
                      direction={getOrderByKey(column.id)?.order}
                      onClick={() => handleRequestSort(column.id)}
                    >
                      {column.label}

                      {/* Если колонка в сортировке, покажем её порядковый номер */}
                      {getOrderByKey(column.id) && (
                        <Box
                          component="span"
                          sx={{
                            ml: 0.5,
                            fontSize: '0.75rem',
                            color: 'text.secondary',
                            backgroundColor: '#e0e0e0',
                            px: 0.5,
                            borderRadius: 1,
                            display: 'inline-block',
                          }}
                        >
                          {order.findIndex((s) => s.key === column.id) + 1}
                        </Box>
                      )}

                      <Box component="span" sx={visuallyHidden}>
                        {getOrderByKey(column.id)?.order === 'desc' ? 'sorted descending' : 'sorted ascending'}
                      </Box>
                    </TableSortLabel>

                  ) : (
                    column.label
                  )}


                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {events?.map((row, index) => {
                return (
                  <TableRow
                    sx={{ backgroundColor: getColor(row.event_priority) }}
                    onClick={() => props.onSelect(row)}
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
                        var utcDate = new Date(row.timestamp + 'Z');
                        value = utcDate.toLocaleString(undefined, { // undefined = использовать локаль браузера
                          year: 'numeric',
                          month: 'numeric',
                          day: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit',
                          second: '2-digit',
                          timeZoneName: 'short' // Покажет название часового пояса
                        }) + " [" + utcDate.getMilliseconds() + "ms]";
                      }

                      if (column.id == 'param0') {
                        value = row.param0;
                      }
                      if (column.id == 'param1') {
                        value = row.param1;
                      }

 
                      return (
                        <TableCell
                          key={column.id}
                          align={column.align}
                          sx={{
                            maxWidth: 170,           // ограничивает ширину
                            whiteSpace: 'nowrap',    // не переносить строки
                            overflow: 'hidden',      // скрыть лишнее
                            textOverflow: 'ellipsis' // троеточие
                          }}
                        >
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