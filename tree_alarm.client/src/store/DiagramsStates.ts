import { IDiagramDTO, IDiagramFullDTO, IDiagramContentDTO, DeepCopy} from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApplicationState } from './index';
import { ApiDiagramsRootString } from './constants';


export interface DiagramsStates {
  cur_diagram_content: IDiagramContentDTO | null; 
  depth: number;
  cur_diagram: IDiagramFullDTO | null;
  diagrams_updated: boolean;
}

const unloadedState: DiagramsStates = {
  cur_diagram_content: null,
  depth: 1,
  cur_diagram: null,
  diagrams_updated: false
};

export const fetchGetDiagramContent = createAsyncThunk<IDiagramContentDTO, string>(
  'diagrams/GetDiagram',
  async (diagram_id: string, { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;
    const diagramsStates = state.diagramsStates as DiagramsStates;

    var fetched = await DoFetch(ApiDiagramsRootString + "/GetDiagramContent?diagram_id=" +
      diagram_id + "&depth=" + diagramsStates.depth,
      {
      method: "GET",
      headers: { "Content-type": "application/json" }
    });

    var json = await fetched.json() as Promise<IDiagramContentDTO>;

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

export const deleteDiagrams = createAsyncThunk<string[], string[]>(
  'diagrams/DeleteDiagrams',
  async (diagramsToUpdate: string[]) => {
    let body = JSON.stringify(diagramsToUpdate);

    var fetched = await DoFetch(ApiDiagramsRootString + "/DeleteDiagrams",
      {
        method: "DELETE",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<string[]>;

    return json;
  }
)

export const fetchSingleDiagram = createAsyncThunk<IDiagramFullDTO, string>(
  'diagrams/fetchSingleDiagram',
  async (diagram_id: string, { rejectWithValue }) => {
    const fetched = await DoFetch(
      `${ApiDiagramsRootString}/GetDiagramFull?diagram_id=${diagram_id}`,
      {
        method: "GET",
        headers: { "Content-type": "application/json" }
      }
    );

    const json = await fetched.json();

    if (!json) {
      return rejectWithValue("Diagram not found");
    }

    return json as IDiagramFullDTO;
  }
);


const diagramsSlice = createSlice({
  name: 'DiagramStates',
  initialState: unloadedState,
  reducers: {

    set_diagram_content_locally(state: DiagramsStates, action: PayloadAction<IDiagramContentDTO|null>) {
      state.cur_diagram_content = action.payload;
    },
    update_single_diagram_locally(state: DiagramsStates, action: PayloadAction<IDiagramFullDTO |null>) {

      var cur_diagram_full = action.payload;
      state.cur_diagram = cur_diagram_full;

      if (state.cur_diagram_content == null ||
        action.payload == null) {
        return;
      }

      var cur_diagram = cur_diagram_full?.diagram ?? null;

      if (cur_diagram) {
        var newContent = state.cur_diagram_content.content.filter(d => d.id != cur_diagram?.id);
        newContent.push(cur_diagram);
        state.cur_diagram_content.content = newContent;
      }      
    },
    set_depth(state: DiagramsStates, action: PayloadAction<number>) {
      state.depth = action.payload;
    },
    remove_ids_locally(state: DiagramsStates, action: PayloadAction<string[]>) {
      if (state.cur_diagram_content != null)
        state.cur_diagram_content.content = state.cur_diagram_content.content.filter(
        d => action.payload.find( id => id==d.id) == null);
    }
  }
  ,
  extraReducers: (builder) => {
    builder
      .addCase(fetchGetDiagramContent.pending, (state, action) => {
        //state.parents = null;
      })
      .addCase(fetchGetDiagramContent.fulfilled, (state, action) => {
        const { requestId } = action.meta
        state.cur_diagram_content = action.payload;
      })
      .addCase(fetchGetDiagramContent.rejected, (state, action) => {
        const { requestId } = action.meta
        state.cur_diagram_content = null;        
      })

    //updateDiagrams
      .addCase(updateDiagrams.pending, (state, action) => {
        state.diagrams_updated = false;
      })
      .addCase(updateDiagrams.fulfilled, (state: DiagramsStates, action) => {
        const updated: IDiagramDTO[] = action.payload;

        var cur_diagram_found = updated.find(ud => ud.id == state.cur_diagram?.diagram?.id);

        if (cur_diagram_found != null && state.cur_diagram != null) {

          var new_diag = DeepCopy(state.cur_diagram) ?? null;

          if (new_diag) {
            new_diag.diagram = cur_diagram_found;
            state.cur_diagram = new_diag;
          }          
        }

        state.diagrams_updated = true;

        if (state.cur_diagram_content == null) {
          return;
        }
        var newContent =
          state.cur_diagram_content.content.filter(d => updated.find(ud => ud.id == d.id) == null);
        state.cur_diagram_content.content = [...updated, ...newContent];
      })
      .addCase(updateDiagrams.rejected, (state, action) => {
        const { requestId } = action.meta
        //state.cur_diagram = null;
      })
      //fetchSingleDiagram
      .addCase(fetchSingleDiagram.fulfilled, (state: DiagramsStates, action) => {
        state.cur_diagram = action.payload;      
      })
      .addCase(fetchSingleDiagram.rejected, (state, action) => {
        state.cur_diagram = null;
      })
    //deleteDiagrams
      .addCase(deleteDiagrams.fulfilled, (state: DiagramsStates, action) => {

        if (state.cur_diagram?.diagram != null && action.payload.find(e => e == state?.cur_diagram?.diagram?.id))
          state.cur_diagram = null;
      })
      .addCase(deleteDiagrams.rejected, (state, action) => {
        state.cur_diagram = null;
      })
  },
})

export const {
  set_depth,
  set_diagram_content_locally,
  remove_ids_locally,
  update_single_diagram_locally
} = diagramsSlice.actions
export const reducer = diagramsSlice.reducer



