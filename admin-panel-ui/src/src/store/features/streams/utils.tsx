import axios from "axios";
import { getEnvConfig } from "../../../common/env";
import { createAsyncThunk } from "@reduxjs/toolkit";

export const streamsAxiosInstance = axios.create({
  baseURL: getEnvConfig().STREAMS_BASE_URI,
});

export const createStreamAsyncThunk = createAsyncThunk.withTypes<{
  rejectValue: string;
}>();
