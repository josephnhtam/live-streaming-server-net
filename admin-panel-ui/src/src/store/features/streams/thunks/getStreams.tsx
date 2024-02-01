import { ActionReducerMapBuilder, EntityState } from "@reduxjs/toolkit";
import { Stream } from "../model";
import axios from "axios";
import { ErrorResponse } from "../../../../common/model";
import { StreamsMetaData, streamsAdapter } from "../slice";
import { createStreamAsyncThunk, streamsAxiosInstance } from "../utils";

interface getStreamsResponse {
  streams: Stream[];
  totalCount: number;
}

export const getStreams = createStreamAsyncThunk(
  "streams/getStreams",

  async (
    params: { page: number; pageSize: number; filter?: string },
    thunkApi
  ) => {
    try {
      const response = await streamsAxiosInstance.get<getStreamsResponse>("/", {
        params: {
          page: params.page,
          pageSize: params.pageSize,
          filter: params.filter,
        },
      });
      return response.data;
    } catch (error) {
      if (axios.isAxiosError<ErrorResponse>(error) && error.response)
        return thunkApi.rejectWithValue(error.response.data.message);

      return thunkApi.rejectWithValue("Failed to get streams.");
    }
  }
);

export function addGetStreamsCases(
  builder: ActionReducerMapBuilder<
    EntityState<Stream, string> & StreamsMetaData
  >
) {
  builder.addCase(getStreams.pending, (state, action) => {
    state.page = action.meta.arg.page;
    state.pageSize = action.meta.arg.pageSize;
  });

  builder.addCase(getStreams.fulfilled, (state, action) => {
    streamsAdapter.setAll(state, action.payload.streams);
    state.totalCount = action.payload.totalCount;
    state.loaded = true;
    state.loading = false;
  });

  builder.addCase(getStreams.rejected, (state, action) => {
    state.error = action.payload!;
    state.loading = false;
    state.revision++;
  });
}
