import { IGetDiagramDTO, TreeMarker } from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApiDiagramsRootString } from '.';


export interface DiagramsStates {
  cur_diagram: IGetDiagramDTO | null;
  //content: TreeMarker[];
}

const unloadedState: DiagramsStates = {
  cur_diagram: null
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
    reset_diagram(state: DiagramsStates, action: PayloadAction<null>) {
      state.cur_diagram = null;
    }
  }
  ,
  extraReducers: (builder) => {
    builder
      .addCase(fetchDiagram.pending, (state, action) => {
        //state.parents = null;
      })
      .addCase(fetchDiagram.fulfilled, (state, action) => {
        const { requestId } = action.meta
        state.cur_diagram = action.payload;
      })
      .addCase(fetchDiagram.rejected, (state, action) => {
        const { requestId } = action.meta
        state.cur_diagram = null;
      })
  },
})

//export const { set_diagram } = diagramsSlice.actions
export const reducer = diagramsSlice.reducer



