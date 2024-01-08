import { IGetDiagramDTO} from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApiDiagramsRootString, ApplicationState } from './index';


export interface DiagramsStates {
  cur_diagram: IGetDiagramDTO | null; 
  depth: number;
}

const unloadedState: DiagramsStates = {
  cur_diagram: null,
  depth: 1
};

export const fetchDiagram = createAsyncThunk<IGetDiagramDTO, string>(
  'diagrams/GetDiagram',
  async (diagram_id: string, { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramsStates = state.diagramsStates as DiagramsStates;

    var fetched = await DoFetch(ApiDiagramsRootString + "/GetDiagramByParent?parent_id=" +
      diagram_id + "&depth=" + diagramsStates.depth,
      {
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

    reset_diagram(state: DiagramsStates, action: PayloadAction<null>) {
      state.cur_diagram = null;
    },
    set_depth(state: DiagramsStates, action: PayloadAction<number>) {
      state.depth = action.payload;
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

export const { reset_diagram, set_depth } = diagramsSlice.actions
export const reducer = diagramsSlice.reducer



