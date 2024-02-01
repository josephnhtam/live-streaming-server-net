import { configureStore } from "@reduxjs/toolkit";
import { StreamsActions, StreamsReducer } from "./features/streams";
import { TypedUseSelectorHook, useDispatch, useSelector } from "react-redux";

export const appStore = configureStore({
  reducer: {
    streams: StreamsReducer,
  },
});

export const AppActions = StreamsActions;

export type AppState = ReturnType<typeof appStore.getState>;
export type AppDispatch = typeof appStore.dispatch;

export const useAppDispatch: () => AppDispatch = useDispatch;
export const useAppSelector: TypedUseSelectorHook<AppState> = useSelector;
