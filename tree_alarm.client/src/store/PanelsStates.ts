import { IPanelsStatesDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'


export interface PanelsStates {
  panels: IPanelsStatesDTO[];
  inited: boolean;
}

const unloadedState: PanelsStates = {
  panels: [],
  inited:false
};

var panels = localStorage.getItem('PanelsStates');

if (panels != null && panels != '') {
  try {
    unloadedState.panels = JSON.parse(panels);
  }
  catch (e: any) {
    unloadedState.panels = [];
  }
  
}

export const updatePannelsStates = createAsyncThunk<IPanelsStatesDTO[], IPanelsStatesDTO[]>(
  'states/UpdatePanelsStates',
  async (panelStates: IPanelsStatesDTO[], thunkAPI) => {

    // THIS IS TODO
    let body = JSON.stringify(panelStates);
    var fetched = await DoFetch("states" + "/UpdatePanelsStates", {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    var json = await fetched.json() as Promise<IPanelsStatesDTO[]>;

    return json;
  }
)

const panelsSlice = createSlice({
  name: 'PanelsStates',
  initialState: unloadedState,
  reducers: {
    // Give case reducers meaningful past-tense "event"-style names
    set_panels(state: PanelsStates, action: PayloadAction<IPanelsStatesDTO[]>) {

      localStorage.setItem('PanelsStates', JSON.stringify(action.payload));

      state.panels = action.payload;
      state.inited = true;
    }
  }
})

export const { set_panels } = panelsSlice.actions
export const reducer = panelsSlice.reducer



