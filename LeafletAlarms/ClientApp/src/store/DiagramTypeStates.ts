import { IDiagramTypeDTO, IGetDiagramTypesDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApiDiagramTypessRootString, ApplicationState } from './index';


export interface DiagramTypeStates {
  cur_diagramtype: IDiagramTypeDTO | null;
}

const unloadedState: DiagramTypeStates = {
  cur_diagramtype: null,
};

export const fetchDiagramTypeByName = createAsyncThunk<IGetDiagramTypesDTO, string>(
  'diagramtypes/GetDiagramTypesByName',
  async (name: string, { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramTypeStates = state.diagramtypeStates as DiagramTypeStates;

    let body = JSON.stringify([name]);

    var fetched = await DoFetch(ApiDiagramTypessRootString + "/GetDiagramTypesByName",
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

    var fetched = await DoFetch(ApiDiagramTypessRootString + "/GetDiagramTypesById",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IGetDiagramTypesDTO>;

    return json;
  }
)

const diagramtypesSlice = createSlice({
  name: 'DiagramTypesStates',
  initialState: unloadedState,
  reducers: {

    reset_diagram(state: DiagramTypeStates, action: PayloadAction<null>) {
      state.cur_diagramtype = null;
    }
  }
  ,
  extraReducers: (builder) => {
    builder
      .addCase(fetchDiagramTypeByName.pending, (state, action) => {
        //state.parents = null;
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

      .addCase(fetchDiagramTypeById.pending, (state, action) => {
        //state.parents = null;
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
  },
})

export const { reset_diagram } = diagramtypesSlice.actions
export const reducer = diagramtypesSlice.reducer



