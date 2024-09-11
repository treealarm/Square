/* eslint-disable react-hooks/exhaustive-deps */
import dayjs from 'dayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { Box, IconButton, List, ListItem, Tooltip } from '@mui/material';
import LibraryAddIcon from '@mui/icons-material/LibraryAdd';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useCallback, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useAppDispatch } from '../store/configureStore';
import { ApplicationState } from '../store';
import * as GuiStore from '../store/GUIStates';
import * as SearchResultStore from '../store/SearchResultStates';
import { PropertyFilter } from './PropertyFilter';
import { SearchFilterGUI, SearchFilterDTO, DeepCopy, ObjPropsSearchDTO } from '../store/Marker';
import { SearchApplyButton } from './SearchApplyButton';
import { ApiDefaultPagingNum, INPUT_DATETIME_FORMAT } from '../store/constants';

export function RetroSearch() {
  const appDispatch = useAppDispatch();

  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);
  const tracks = useSelector((state: ApplicationState) => state?.tracksStates?.tracks);
  const routes = useSelector((state: ApplicationState) => state?.tracksStates?.routes);


  const updateFilterInRedux = (filter: SearchFilterGUI, applyFilter: boolean) => {
    filter.search_id = new Date().toISOString();
    filter.applied = applyFilter;
    appDispatch(GuiStore.applyFilter(filter));
    if (!filter.applied) {
      appDispatch(SearchResultStore.setEmptyResult());
    }
  };


  const doSearchTracks = useCallback((filterIn: SearchFilterGUI) => {
    if (!filterIn) {
      console.error("DoSearchTracks:", "filterIn is null");
      return;
    }   

    const filterDto: SearchFilterDTO = {
      search_id: new Date().toISOString(),
      property_filter: filterIn.property_filter,
      time_start: filterIn.time_start,
      time_end: filterIn.time_end,
      forward: 1,
      count: ApiDefaultPagingNum,
    };

    if (!filterIn.applied) {
      appDispatch(SearchResultStore.setEmptyResult());
    } else {
      appDispatch(SearchResultStore.fetchTracksByFilter(filterDto));
    }
  }, []);

  ////////////////////
  const handleChangeStart = useCallback((newValue: dayjs.Dayjs | null) => {
    if (newValue) {
      const filter = DeepCopy(searchFilter);
      if (filter) {
        filter.time_start = newValue.toISOString();
        updateFilterInRedux(filter, false);
      }
    }
  }, [searchFilter]);
  ////////////////////
  const handleChangeEnd = useCallback((newValue: dayjs.Dayjs | null) => {
    if (newValue) {
      const filter = DeepCopy(searchFilter);
      if (filter) {
        filter.time_end = newValue.toISOString();
        updateFilterInRedux(filter, false);
      }
    }
  }, [searchFilter]);
  ////////////////////
  const addProperty = useCallback(() => {
    const filter = DeepCopy(searchFilter);
    if (filter && filter.property_filter && filter.property_filter.props) {
      filter.property_filter.props.push({ prop_name: "test_name", str_val: "test_val" });
      updateFilterInRedux(filter, false);
    }
  }, [searchFilter]);
  ////////////////////
  const setPropsFilter = useCallback((propsFilter: ObjPropsSearchDTO) => {
    const filter = DeepCopy(searchFilter);
    if (filter) {
      filter.property_filter = propsFilter;
      updateFilterInRedux(filter, false);
    }
  }, [searchFilter]);
  ////////////////////
  const handleNavigate = useCallback((next: boolean) => {
    if (!tracks && !routes) return;

    const filter = DeepCopy(searchFilter);
    if (!filter) return;

    let minDate = next ? new Date('2045-10-05T11:03:21') : new Date('1945-01-01T00:00:00');
    let maxDate = minDate;

    if (tracks) {
      tracks.forEach(track => {
        const curTs = new Date(track.timestamp);
        if (next ? curTs < minDate : curTs > maxDate) {
          minDate = curTs;
        }
      });
    }

    if (routes) {
      routes.forEach(route => {
        const curTs = new Date(route.ts_start);
        if (next ? curTs < minDate : curTs > maxDate) {
          minDate = curTs;
        }
      });
    }

    if (next) {
      filter.time_start = dayjs(minDate).add(1, 's').toISOString();
    } else {
      filter.time_end = dayjs(maxDate).subtract(1, 's').toISOString();
      filter.sort = -1;
    }

    updateFilterInRedux(filter, filter.applied == true);
  }, [tracks, routes, doSearchTracks, searchFilter]);
  ////////////////////

  useEffect(() => {
    const filter = DeepCopy(searchFilter);
    if (filter) {
      doSearchTracks(filter);
    }
  }, [searchFilter]);

  
  var time_start = dayjs(searchFilter?.time_start);
  console.log(time_start);
  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <List dense>
        <ListItem>
          <Box display="flex" justifyContent="flex-start">
            <SearchApplyButton hideIfNotPushed={false} />
          </Box>
        </ListItem>
        <ListItem>
          <DateTimePicker
            label="begin"
            value={time_start}
            onChange={handleChangeStart}
            views={['year', 'month', 'day', 'hours', 'minutes', 'seconds']}
            format={INPUT_DATETIME_FORMAT}
          />
          <Tooltip title="Shift begin time to next track">
            <IconButton onClick={() => handleNavigate(true)}>
              <ArrowForwardIcon />
            </IconButton>
          </Tooltip>
        </ListItem>
        <ListItem>
          <DateTimePicker
            label="end"
            value={dayjs(searchFilter?.time_end)}
            onChange={handleChangeEnd}
            format={INPUT_DATETIME_FORMAT}
          />
          <Tooltip title="Shift end time to previous track">
            <IconButton onClick={() => handleNavigate(false)}>
              <ArrowBackIcon />
            </IconButton>
          </Tooltip>
        </ListItem>
        <ListItem>
          <Box display="flex" justifyContent="flex-start">
            <Tooltip title="Add property value into the filter">
              <IconButton aria-label="addProp" size="medium" onClick={addProperty}>
                <LibraryAddIcon fontSize="inherit" />
              </IconButton>
            </Tooltip>
          </Box>
        </ListItem>
        <ListItem>
          <PropertyFilter
            propsFilter={searchFilter?.property_filter}
            setPropsFilter={setPropsFilter}
          />
        </ListItem>
      </List>
    </LocalizationProvider>
  );
}
