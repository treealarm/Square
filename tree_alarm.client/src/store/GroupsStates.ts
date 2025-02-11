import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { DoFetch } from './Fetcher';
import { ApiGroupsRootString } from './constants';
import { IGroupDTO } from './Marker';


export interface GroupState {
  groups: IGroupDTO[] | null;
  isFetching: boolean;
}

const initialState: GroupState = {
  groups: null,
  isFetching: false,
};

export const fetchGroupsByNames = createAsyncThunk<IGroupDTO[], string[]>(
  'groups/GetByNames',
  async (names) => {
    const response = await DoFetch(ApiGroupsRootString + '/GetByNames', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(names),
    });
    return response.json();
  }
);

export const updateGroups = createAsyncThunk<IGroupDTO[], IGroupDTO[]>(
  'groups/UpdateGroups',
  async (groups) => {
    await DoFetch(ApiGroupsRootString + '/UpdateGroups', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(groups),
    });
    return groups;
  }
);

export const deleteGroups = createAsyncThunk<void, string[]>(
  'groups/DeleteGroups',
  async (ids) => {
    await DoFetch(ApiGroupsRootString + '/DeleteGroups', {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(ids),
    });
  }
);

export const deleteGroupsByNames = createAsyncThunk<void, string[]>(
  'groups/DeleteGroupsByNames',
  async (names) => {
    await DoFetch(ApiGroupsRootString + '/DeleteGroupsByNames', {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(names),
    });
  }
);

const groupSlice = createSlice({
  name: 'groups',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchGroupsByNames.pending, (state) => {
        state.isFetching = true;
      })
      .addCase(fetchGroupsByNames.fulfilled, (state, action) => {
        state.isFetching = false;
        state.groups = action.payload;
      })
      .addCase(fetchGroupsByNames.rejected, (state) => {
        state.isFetching = false;
      })
      .addCase(updateGroups.fulfilled, (state, action) => {
        state.groups = action.payload;
      })
      .addCase(deleteGroups.fulfilled, (state) => {
        state.groups = null;
      })
      .addCase(deleteGroupsByNames.fulfilled, (state) => {
        state.groups = null;
      });
  },
});

export const reducer = groupSlice.reducer;
