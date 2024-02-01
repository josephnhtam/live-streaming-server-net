import * as React from "react";
import Box from "@mui/joy/Box";
import FormControl from "@mui/joy/FormControl";
import FormLabel from "@mui/joy/FormLabel";
import Input from "@mui/joy/Input";
import SearchIcon from "@mui/icons-material/Search";
import {
  StreamTableContext,
  IStreamTableContext,
  useStreamTableContext,
} from "../context/StreamTableContext";
import IconButton from "@mui/joy/IconButton";
import Table, { HeadCell } from "../../../components/table";
import RefreshIcon from "@mui/icons-material/Refresh";
import { Stream } from "../../../store/features/streams";
import {
  ResolutionColumn,
  StartTimeColumn,
  StreamOptionsColumn,
  StreamPathColumn,
} from "./CustomNodes";

const headCells: HeadCell<Stream, IStreamTableContext>[] = [
  {
    id: "streamPath",
    label: "Stream Path",
    width: "256px",
    customNode: StreamPathColumn,
  },

  {
    id: "videoCodecId",
    label: "Video Codec",
    width: "130px",
  },

  {
    id: "width",
    label: "Resolution",
    width: "130px",
    customNode: ResolutionColumn,
  },

  {
    id: "framerate",
    label: "Framerate",
    width: "130px",
  },

  {
    id: "audioCodecId",
    label: "Audio Codec",
    width: "130px",
  },

  {
    id: "audioSampleRate",
    label: "Sample Rate",
    width: "130px",
  },

  {
    id: "audioChannels",
    label: "Channels",
    width: "130px",
  },

  {
    id: "subscribersCount",
    label: "Subscribers",
    width: "130px",
  },

  {
    id: "startTime",
    label: "Start Time",
    width: "160px",
    customNode: StartTimeColumn,
  },

  {
    id: "id",
    align: "right",
    label: "",
    width: "90px",
    customNode: StreamOptionsColumn,
  },
];

export default function StreamTable() {
  const streamTableContext = useStreamTableContext();

  const onRefresh = () => {
    streamTableContext.refetch();
  };

  const onChangeFilter = (ev: React.ChangeEvent<HTMLInputElement>) => {
    streamTableContext.setFilter(ev.target.value);
  };

  return (
    <StreamTableContext.Provider value={streamTableContext}>
      <Box className="SearchAndFilters-tabletUp flex pb-3 gap-2 [&>*]:min-w-32 md:[&>*]:min-w-40">
        <FormControl className="flex-1" size="sm">
          <FormLabel>Search for stream</FormLabel>
          <Input
            size="sm"
            placeholder="Search"
            startDecorator={<SearchIcon />}
            value={streamTableContext.filter}
            onChange={onChangeFilter}
          />
        </FormControl>

        <Box className="!min-w-0 flex flex-col justify-end">
          <IconButton
            size="sm"
            className="w-2 h-8"
            variant="outlined"
            onClick={onRefresh}
          >
            <RefreshIcon />
          </IconButton>
        </Box>
      </Box>

      <Table
        headCells={headCells}
        context={StreamTableContext}
        isSelectable={false}
        sx={{
          flex: 1,
          td: {
            borderBottomWidth: "1px",
          },
        }}
      />
    </StreamTableContext.Provider>
  );
}
