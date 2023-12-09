import { IGetDiagramDTO, TreeMarker } from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApiDiagramsRootString } from '.';


export interface DiagramsStates {
  cur_diagram_id: string | null;
  parents: TreeMarker[];
  //content: TreeMarker[];
}

const unloadedState: DiagramsStates = {
  parents: [],
  cur_diagram_id: null
};

export const fetchDiagram = createAsyncThunk<IGetDiagramDTO, string>(
  'diagrams/GetDiagram',
  async (diagram_id: string, thunkAPI) => {

    var fetched = await DoFetch(ApiDiagramsRootString + "/GetDiagram?diagram_id=" +
      diagram_id, {
      method: "GET",
      headers: { "Content-type": "application/json" }
    });

    var json = await fetched.json() as Promise<IGetDiagramDTO>;

    return json;
  }
)

const diagramsSlice = createSlice({
  name: 'DiagramStates',
  initialState: unloadedState,
  reducers: {
  //  // Give case reducers meaningful past-tense "event"-style names
  //  set_diagram(state: DiagramsStates, action: PayloadAction<string>) {
  //    state.cur_diagram_id = action.payload;
  //  }
  }
  ,
  extraReducers: (builder) => {
    builder
      .addCase(fetchDiagram.pending, (state, action) => {
        //state.parents = null;
      })
      .addCase(fetchDiagram.fulfilled, (state, action) => {
        const { requestId } = action.meta
        state.parents = action.payload.parents;
        state.cur_diagram_id = action.payload.cur_diagram_id;
      })
      .addCase(fetchDiagram.rejected, (state, action) => {
        const { requestId } = action.meta
        state.parents = null;
      })
  },
})

//export const { set_diagram } = diagramsSlice.actions
export const reducer = diagramsSlice.reducer



