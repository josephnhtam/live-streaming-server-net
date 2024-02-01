import * as React from "react";
import Box from "@mui/joy/Box";
import Typography from "@mui/joy/Typography";
import ConnectedTvIcon from "@mui/icons-material/ConnectedTv";
import Main from "../../layout/Main";
import StreamTable from "./components/StreamTable";
import {
  StreamPreviewContext,
  useStreamPreviewContext,
} from "./context/StreamPreviewContext";
import StreamPreview from "./components/StreamPreview";

export default function Streams() {
  const streamPreviewContext = useStreamPreviewContext();

  return (
    <StreamPreviewContext.Provider value={streamPreviewContext}>
      <StreamPreview />
      <Main scrollable={false}>
        <Box className="h-full flex flex-col">
          <Box className="flex items-center pb-4">
            <ConnectedTvIcon className="text-2xl mr-2" />
            <Typography level="h3" component="h1">
              Streams
            </Typography>
          </Box>
          <StreamTable />
        </Box>
      </Main>
    </StreamPreviewContext.Provider>
  );
}
