import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'

import { IEventDTO, SearchEventFilterDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { ApiDefaultPagingNum, ApiEventsRootString } from './constants';



export interface EventStates {
  events: IEventDTO[] | null;
  filter: SearchEventFilterDTO;
  selected_event: IEventDTO | null;
  isFetching: boolean;
}

const unloadedState: EventStates = {
  events: null,
  filter: {
    search_id: null,
    property_filter: null,
    time_start: null,
    time_end: null,
    forward: 0,
    count: ApiDefaultPagingNum,
    sort: [{ key: 'timestamp', order: 'desc' }],
    images_only:false
  },
  selected_event: null,
  isFetching: false
};

export const fetchEventsByFilter = createAsyncThunk<IEventDTO[], SearchEventFilterDTO>(
  'events/GetByFilter',
  async (filter: SearchEventFilterDTO, { getState, dispatch }) => {

    //dispatch(setIsFetching(true));

    let body = JSON.stringify(filter);   

    var fetched = await DoFetch(ApiEventsRootString + "/GetByFilter",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IEventDTO[]>;

    return json;
  }
)

const eventsSlice = createSlice({
  name: 'EventsStates',
  initialState: unloadedState,
  reducers: {
    set_local_filter(state: EventStates, action: PayloadAction<SearchEventFilterDTO>) {
      state.filter = action.payload;
    },
    set_selected_event(state: EventStates, action: PayloadAction<IEventDTO|null>) {
      state.selected_event = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchEventsByFilter.pending, (state: EventStates, action) => {
        state.isFetching = true;
      })
      .addCase(fetchEventsByFilter.fulfilled, (state: EventStates, action) => {
        state.isFetching = false;
        const { requestId } = action.meta
        state.events = action.payload;
      })
      .addCase(fetchEventsByFilter.rejected, (state: EventStates, action) => {
        state.isFetching = false;
        const { requestId } = action.meta
        state.events = null;
      })
  },
})

export const { set_local_filter, set_selected_event } = eventsSlice.actions
export const reducer = eventsSlice.reducer



