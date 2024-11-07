import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApiRootString } from './constants';
import { DoFetch } from './Fetcher';
import { DeepCopy, GetByParentDTO, TreeMarker, Marker } from './Marker';

export interface TreeState {
  isLoading: boolean;
  parent_id: string | null;
  parents: (TreeMarker | null)[];
  children: TreeMarker[];
  parentBounds: Record<string, { start_id: string | null; end_id: string | null }>; // словарь с parent_id в качестве ключа
}


// Initial state
const initialState: TreeState = {
  children: [],
  isLoading: false,
  parent_id: null,
  parents: [],
  parentBounds: {}
};

export async function getByParent(parent_id: string | null, start_id: string | null, end_id: string | null): Promise<GetByParentDTO> {

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

  const response = await DoFetch(request, { method: "GET" });

  if (!response.ok) {
    throw new Error('Failed to fetch data');
  }

  const json = await response.json() as GetByParentDTO;

  return json;
}

export async function getById(id: string | null): Promise <Marker>
{

    let request = `${ApiRootString}/GetById?id=${id}`;


    var fetched = await DoFetch(request,
      {
        method: "GET"
      });

    var json = await fetched.json() as Promise<Marker>;

    return json;
 }


// Async thunk for fetching data
export const fetchByParent = createAsyncThunk < GetByParentDTO, { parent_id: string | null, start_id: string | null, end_id:string | null} >(
  'tree/fetchByParent',
  async ({ parent_id, start_id, end_id }) => {

    return getByParent(parent_id, start_id, end_id);
  }
);

export const fetchById = createAsyncThunk < Marker, string | null>(
  'tree/fetchById',
  async (id) => {
    return getById(id);
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

      const updatedChildren = DeepCopy(state.children)?.map(item =>
        item.id === treeItem.id ? { ...item, name: treeItem.name } : item
      );

      state.children = updatedChildren ?? [];
    },
    setParentIdLocally: (state, action: PayloadAction<string | null>) => {
      state.parent_id = action.payload;
    }
  },
  
  extraReducers: (builder) => {
    builder
      .addCase(fetchByParent.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(fetchByParent.fulfilled, (state, action: PayloadAction<GetByParentDTO>) => {
        const data = action.payload;
        state.parent_id = data.parent_id ?? null;
        state.children = data.children ?? [];
        state.parents = [null, ...(data.parents ?? [])];
        // Обновляем start_id и end_id для текущего parent_id
        //if (data.parent_id) {
          state.parentBounds[data.parent_id ?? ''] = {
            start_id: data.start_id ?? null,
            end_id: data.end_id ?? null,
          //};
        }
        state.isLoading = false;
      })
      .addCase(fetchByParent.rejected, (state, action) => {
        console.log('getByParent:', action.error.message);
        state.isLoading = false;
      })
      //getById
      .addCase(fetchById.fulfilled, (state, action: PayloadAction<Marker>) => {
      })
      ;
  }
});

export const { setTreeItem, setParentIdLocally } = treeSlice.actions;
export const reducer = treeSlice.reducer;
