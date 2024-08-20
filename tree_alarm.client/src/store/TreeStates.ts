import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApiRootString } from './constants';
import { DoFetch } from './Fetcher';
import { DeepCopy, GetByParentDTO, TreeMarker, Marker } from './Marker';

export interface TreeState {
  isLoading: boolean;
  parent_id: string | null;
  parents: TreeMarker[];
  children: TreeMarker[];
  start_id?: string;
  end_id?: string;
}

// Initial state
const initialState: TreeState = {
  children: [],
  isLoading: false,
  parent_id: null,
  parents: []
};

// Async thunk for fetching data
export const getByParent = createAsyncThunk < GetByParentDTO, { parent_id: string | null, start_id: string | null, end_id:string | null} >(
  'tree/getByParent',
  async ({ parent_id, start_id, end_id }) => {

    console.log('getByParent:', parent_id);

    console.log('fetching by parent_id=', parent_id);
    let request = `${ApiRootString}/GetByParent?count=100`;

    if (parent_id != null) {
      request += `&parent_id=${parent_id}`;
    }

    if (start_id != null) {
      request += `&start_id=${start_id}`;
    }

    if (end_id != null) {
      request += `&end_id=${end_id}`;
    }

    var fetched = await DoFetch(request,
      {
        method: "GET"
      });

    var json = await fetched.json() as Promise<GetByParentDTO>;

    return json;
  }
);

export const getById = createAsyncThunk < Marker, string | null>(
  'tree/getById',
  async (id) => {

    let request = `${ApiRootString}/GetById?id=${id}`;

   
    var fetched = await DoFetch(request,
      {
        method: "GET"
      });

    var json = await fetched.json() as Promise<Marker>;

    return json;
  }
);

// Slice
const treeSlice = createSlice({
  name: 'tree',
  initialState,
  reducers: {
    setTreeItem: (state, action: PayloadAction<TreeMarker | null>) => {
      const treeItem = action.payload;
      if (!treeItem) return;

      const found = state.children.find(i => i.id === treeItem.id);
      if (!found) return;

      const updatedChildren = DeepCopy(state.children).map(item =>
        item.id === treeItem.id ? { ...item, name: treeItem.name } : item
      );

      state.children = updatedChildren;
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(getByParent.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(getByParent.fulfilled, (state, action: PayloadAction<GetByParentDTO>) => {
        const data = action.payload;
        if (data.parent_id === state.parent_id) {
          state.parent_id = data.parent_id;
          state.children = data.children??[];
          state.parents = data.parents??[];
          state.start_id = data.start_id;
          state.end_id = data.end_id;
        }
        state.isLoading = false;
      })
      .addCase(getByParent.rejected, (state, action) => {
        console.log('getByParent:', action.error.message);
        state.isLoading = false;
      })
      //getById
      .addCase(getById.fulfilled, (state, action: PayloadAction<GetByParentDTO>) => {
      })
      ;
  }
});

export const { setTreeItem } = treeSlice.actions;
export const reducer = treeSlice.reducer;
