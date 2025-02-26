﻿import { IDiagramTypeDTO, IGetDiagramTypesByFilterDTO, IGetDiagramTypesDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApplicationState } from './index';
import { ApiDiagramTypesRootString } from './constants';


export interface DiagramTypeStates {
  cur_diagramtype: IDiagramTypeDTO | null;
  diagramtypes: IDiagramTypeDTO[];
  localFilter: string | null;
  result: string | null;
}

const unloadedState: DiagramTypeStates = {
  cur_diagramtype: null,
  diagramtypes: [],
  localFilter: null,
  result: null
};

export const fetchDiagramTypeByName = createAsyncThunk<IGetDiagramTypesDTO, string>(
  'diagramtypes/GetDiagramTypesByName',
  async (name: string, { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramTypeStates = state.diagramtypeStates as DiagramTypeStates;

    let body = JSON.stringify([name]);

    var fetched = await DoFetch(ApiDiagramTypesRootString + "/GetDiagramTypesByName",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IGetDiagramTypesDTO>;

    return json;
  }
)

export const fetchDiagramTypeById = createAsyncThunk<IGetDiagramTypesDTO, string>(
  'diagramtypes/GetDiagramTypesById',
  async (id: string, { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramTypeStates = state.diagramtypeStates as DiagramTypeStates;

    let body = JSON.stringify([id]);

    var fetched = await DoFetch(ApiDiagramTypesRootString + "/GetDiagramTypesById",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IGetDiagramTypesDTO>;

    return json;
  }
)

export const fetchGetDiagramTypesByFilter = createAsyncThunk<IGetDiagramTypesDTO, IGetDiagramTypesByFilterDTO>(
  'diagramtypes/GetDiagramTypesByFilter',
  async (filter: IGetDiagramTypesByFilterDTO, { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramTypeStates = state.diagramtypeStates as DiagramTypeStates;

    let body = JSON.stringify(filter);

    var fetched = await DoFetch(ApiDiagramTypesRootString + "/GetDiagramTypesByFilter",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IGetDiagramTypesDTO>;

    return json;
  }
)

export const updateDiagramTypes = createAsyncThunk<IGetDiagramTypesDTO, IDiagramTypeDTO[]>(
  'diagramtypes/UpdateDiagramTypes',
  async (diagramsToUpdate: IDiagramTypeDTO[], { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramTypeStates = state.diagramtypeStates as DiagramTypeStates;

    let body = JSON.stringify(diagramsToUpdate);

    var fetched = await DoFetch(ApiDiagramTypesRootString + "/UpdateDiagramTypes",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IGetDiagramTypesDTO>;

    return json;
  }
)

export const deleteDiagramTypes = createAsyncThunk<string[], string[]>(
  'diagramtypes/DeleteDiagramTypes',
  async (diagramTypes2Delete: string[], { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramTypeStates = state.diagramtypeStates as DiagramTypeStates;

    let body = JSON.stringify(diagramTypes2Delete);

    var fetched = await DoFetch(ApiDiagramTypesRootString + "/DeleteDiagramTypes",
      {
        method: "DELETE",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<string[]>;

    return json;
  }
)

const diagramtypesSlice = createSlice({
  name: 'DiagramTypesStates',
  initialState: unloadedState,
  reducers: {

    set_local_diagram(state: DiagramTypeStates, action: PayloadAction<IDiagramTypeDTO>) {
      state.cur_diagramtype = action.payload;
    },
    set_local_filter(state: DiagramTypeStates, action: PayloadAction<string>) {
      state.localFilter = action.payload;
    },
    set_result(state: DiagramTypeStates, action: PayloadAction<string|null>) {
      state.result = action.payload;
    }
  }
  ,
  extraReducers: (builder) => {
    builder
      .addCase(fetchDiagramTypeByName.pending, (state, action) => {
      })
      .addCase(fetchDiagramTypeByName.fulfilled, (state, action) => {
        const { requestId } = action.meta
        if (action.payload?.dgr_types.length > 0)
          state.cur_diagramtype = action.payload.dgr_types[0];
        else
          state.cur_diagramtype = null;
      })
      .addCase(fetchDiagramTypeByName.rejected, (state, action) => {
        const { requestId } = action.meta
        state.cur_diagramtype = null;
      })


      .addCase(fetchDiagramTypeById.fulfilled, (state, action) => {
        const { requestId } = action.meta
        if (action.payload?.dgr_types.length > 0)
          state.cur_diagramtype = action.payload.dgr_types[0];
        else
          state.cur_diagramtype = null;
      })
      .addCase(fetchDiagramTypeById.rejected, (state, action) => {
        const { requestId } = action.meta
        state.cur_diagramtype = null;
      })


      .addCase(fetchGetDiagramTypesByFilter.fulfilled, (state, action) => {
        const { requestId } = action.meta       
        state.diagramtypes = action.payload.dgr_types;
      })
      .addCase(fetchGetDiagramTypesByFilter.rejected, (state, action) => {
        const { requestId } = action.meta
        state.cur_diagramtype = null;
      })

      //updateDiagramTypes

      .addCase(updateDiagramTypes.fulfilled, (state, action) => {
        const { requestId } = action.meta
        if (action.payload?.dgr_types.length > 0) {
          var newDiagram = action.payload.dgr_types[0];
          state.cur_diagramtype = newDiagram;
          if (!state.diagramtypes) {
            state.diagramtypes = [];
          }
          var index = state.diagramtypes?.findIndex(e => e.id == newDiagram.id) ?? -1;
          if (index >= 0) {
            state.diagramtypes[index] = newDiagram;
          }
          else {
            state.diagramtypes.push(newDiagram);
          }
        }          
      })

      // delete      //updateDiagramTypes

      .addCase(deleteDiagramTypes.fulfilled, (state, action) => {
          const { requestId } = action.meta
        if (action.payload?.length > 0) {
          state.cur_diagramtype = null;
          var newArr = state.diagramtypes.filter(e => action.payload.indexOf(e.id) < 0);
          state.diagramtypes = newArr;
          }
        })
  },
})

export const { set_local_diagram, set_local_filter, set_result } = diagramtypesSlice.actions
export const reducer = diagramtypesSlice.reducer



