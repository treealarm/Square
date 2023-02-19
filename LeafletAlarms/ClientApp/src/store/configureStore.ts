import { connectRouter} from 'connected-react-router';
import { History } from 'history';
import { ApplicationState, reducers } from './';
import { configureStore } from '@reduxjs/toolkit'

export default function configureMyStore(history: History, initialState?: ApplicationState) {

  // Automatically adds the thunk middleware and the Redux DevTools extension
  const store = configureStore({
    // Automatically calls `combineReducers`
    reducer: {
      ...reducers,
      router: connectRouter(history)
    }
  });

  return store;
}
