import { ActionReducerMapBuilder, EntityState } from "@reduxjs/toolkit";
import { Stream } from "../model";
import axios from "axios";
import { ErrorResponse } from "../../../../common/model";
import { StreamsMetaData } from "../slice";
import { createStreamAsyncThunk, streamsAxiosInstance } from "../utils";

export const deleteStream = createStreamAsyncThunk(
  "streams/deleteStream",

  async (params: { streamId: string }, thunkApi) => {
    try {
      const response = await streamsAxiosInstance.delete("/", {
        params: {
          streamId: params.streamId,
        },
      });
      return response.data;
    } catch (error) {
      if (axios.isAxiosError<ErrorResponse>(error) && error.response)
        return thunkApi.rejectWithValue(error.response.data.message);

      return thunkApi.rejectWithValue("Failed to delete stream.");
    }
  }
);

export function addDeleteStreamsCases(
  builder: ActionReducerMapBuilder<
    EntityState<Stream, string> & StreamsMetaData
  >
) {
  builder.addCase(deleteStream.fulfilled, (state) => {
    state.revision = state.revision + 1;
  });
}
