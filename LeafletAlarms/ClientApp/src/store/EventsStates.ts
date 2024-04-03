import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'

import { IEventDTO, SearchFilterDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { ApiDefaultPagingNum, ApiEventsRootString } from './constants';



export interface EventStates {
  events: IEventDTO[] | null;
  filter: SearchFilterDTO;
  selected_event: IEventDTO|null;
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
    sort: []
  },
  selected_event: null
};

export const fetchEventsByFilter = createAsyncThunk<IEventDTO[], SearchFilterDTO>(
  'events/GetByFilter',
  async (filter: SearchFilterDTO, { getState }) => {
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

export const reserveCursor = createAsyncThunk<number, string>(
  'events/ReserveCursor',
  async (search_id: string, { getState }) => {
    let body = JSON.stringify(search_id);

    var fetched = await DoFetch(ApiEventsRootString + "/ReserveCursor?search_id=" + search_id,
      {
        method: "GET"
      });

    var json = await fetched.text() as unknown as Promise<number>;

    return json;
  }
)

const eventsSlice = createSlice({
  name: 'EventsStates',
  initialState: unloadedState,
  reducers: {
    set_local_filter(state: EventStates, action: PayloadAction<SearchFilterDTO>) {
      state.filter = action.payload;
    },
    set_selected_event(state: EventStates, action: PayloadAction<IEventDTO|null>) {
      state.selected_event = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchEventsByFilter.pending, (state, action) => {
      })
      .addCase(fetchEventsByFilter.fulfilled, (state, action) => {
        const { requestId } = action.meta
        state.events = action.payload;
      })
      .addCase(fetchEventsByFilter.rejected, (state, action) => {
        const { requestId } = action.meta
        state.events = null;
      })
      //ReserveCursor
      .addCase(reserveCursor.pending, (state, action) => {
      })
      .addCase(reserveCursor.fulfilled, (state, action) => {
      })
      .addCase(reserveCursor.rejected, (state, action) => {
      })
  },
})

export const { set_local_filter, set_selected_event } = eventsSlice.actions
export const reducer = eventsSlice.reducer



