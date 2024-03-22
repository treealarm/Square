import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'

import { IEventDTO, SearchFilterDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { ApiDefaultPagingNum, ApiEventsRootString } from './constants';



export interface EventStates {
  events: IEventDTO[] | null;
  filter: SearchFilterDTO;
}

const unloadedState: EventStates = {
  events: null,
  filter: {
    search_id: (new Date()).toISOString(),
    property_filter: null,
    time_start: null,
    time_end: null,
    forward: true,
    count: ApiDefaultPagingNum,
    sort: {}
  },
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


const eventsSlice = createSlice({
  name: 'EventsStates',
  initialState: unloadedState,
  reducers: {
    set_local_filter(state: EventStates, action: PayloadAction<SearchFilterDTO>) {
      state.filter = action.payload;
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
  },
})

export const { set_local_filter } = eventsSlice.actions
export const reducer = eventsSlice.reducer



