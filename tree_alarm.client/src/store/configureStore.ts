import { reducers } from './';
import { configureStore } from '@reduxjs/toolkit'
import { useDispatch } from 'react-redux';

export default function configureTheStore() {

  // Automatically adds the thunk middleware and the Redux DevTools extension
  const store = configureStore({
    // Automatically calls `combineReducers`
    reducer: reducers
  });

  return store;
}

export var theStore = configureTheStore();
export const useAppDispatch = () => useDispatch<typeof theStore.dispatch>();
