import { ApplicationState, reducers } from './';
import { configureStore } from '@reduxjs/toolkit'

export default function configureTheStore() {

  // Automatically adds the thunk middleware and the Redux DevTools extension
  const store = configureStore({
    // Automatically calls `combineReducers`
    reducer: reducers
  });

  return store;
}
