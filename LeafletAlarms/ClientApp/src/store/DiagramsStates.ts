import { IDiagramDTO, IGetDiagramDTO} from './Marker';
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

export const updateDiagrams = createAsyncThunk<IDiagramDTO[], IDiagramDTO[]>(
  'diagrams/UpdateDiagrams',
  async (diagramsToUpdate: IDiagramDTO[], { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramsStates = state.diagramsStates as DiagramsStates;

    let body = JSON.stringify(diagramsToUpdate);

    var fetched = await DoFetch(ApiDiagramsRootString + "/UpdateDiagrams",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IDiagramDTO[]>;

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
    set_local_diagram(state: DiagramsStates, action: PayloadAction<IGetDiagramDTO>) {
      state.cur_diagram = action.payload;
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

    //updateDiagrams
      .addCase(updateDiagrams.pending, (state, action) => {
        //state.parents = null;
      })
      .addCase(updateDiagrams.fulfilled, (state: DiagramsStates, action) => {
        const { requestId } = action.meta
        const updated: IDiagramDTO[] = action.payload;
        var newContent =
          state.cur_diagram.content.filter(d => updated.find(ud => ud.id == d.id) == null);
        state.cur_diagram.content = [...updated, ...newContent];
      })
      .addCase(updateDiagrams.rejected, (state, action) => {
        const { requestId } = action.meta
        //state.cur_diagram = null;
      })
  },
})

export const { reset_diagram, set_depth, set_local_diagram } = diagramsSlice.actions
export const reducer = diagramsSlice.reducer



