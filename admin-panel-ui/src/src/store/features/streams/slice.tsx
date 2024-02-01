import {
  PayloadAction,
  createEntityAdapter,
  createSlice,
} from "@reduxjs/toolkit";
import { Stream } from "./model";
import { addGetStreamsCases } from "./thunks/getStreams";
import { addDeleteStreamsCases } from "./thunks/deleteStream";

export const streamsAdapter = createEntityAdapter<Stream, string>({
  selectId: (stream) => stream.id,
});

export interface StreamsMetaData {
  totalCount: number;
  loaded: boolean;
  loading: boolean;
  error: string | null;
  revision: number;
  page: number;
  pageSize: number;
}

const initialState = streamsAdapter.getInitialState<StreamsMetaData>({
  totalCount: 0,
  loaded: false,
  loading: false,
  error: null,
  revision: 0,
  page: 1,
  pageSize: 1,
});

const streamsSlice = createSlice({
  name: "streams",
  initialState: initialState,

  reducers: {
    clearError(state) {
      state.error = null;
    },

    add(state, action: PayloadAction<Stream>) {
      if (!state.loaded) return;
      streamsAdapter.addOne(state, action.payload);
    },

    remove(state, action: PayloadAction<string>) {
      if (!state.loaded) return;
      streamsAdapter.removeOne(state, action.payload);
    },
  },

  extraReducers: (builder) => {
    addGetStreamsCases(builder);
    addDeleteStreamsCases(builder);
  },
});

export const StreamsSelector = streamsAdapter.getSelectors();
export const StreamsActions = streamsSlice.actions;
export const StreamsReducer = streamsSlice.reducer;
