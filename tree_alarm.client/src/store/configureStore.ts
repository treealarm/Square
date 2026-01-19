import { reducers } from './';
import { configureStore } from '@reduxjs/toolkit';

import { useDispatch, useSelector, type TypedUseSelectorHook } from 'react-redux';
export default function configureTheStore() {

  // Automatically adds the thunk middleware and the Redux DevTools extension
  const store = configureStore({
    // Automatically calls `combineReducers`
    reducer: reducers
  });

  return store;
}

export var theStore = configureTheStore();

export type RootState = ReturnType<typeof theStore.getState>;
export type AppDispatch = typeof theStore.dispatch;

//export const useAppDispatch = () => useDispatch<typeof theStore.dispatch>();
export const useAppDispatch: () => AppDispatch = useDispatch;
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;