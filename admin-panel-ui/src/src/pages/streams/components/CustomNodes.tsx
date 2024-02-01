import * as React from "react";
import Box from "@mui/joy/Box";
import IconButton from "@mui/joy/IconButton";
import {
  Divider,
  Dropdown,
  Menu,
  MenuButton,
  MenuItem,
  Stack,
  Tooltip,
  Typography,
} from "@mui/joy";
import MoreHorizRoundedIcon from "@mui/icons-material/MoreHorizRounded";
import { Stream } from "../../../store/features/streams";
import moment from "moment";
import { IStreamTableContext } from "../context/StreamTableContext";
import { getEnvConfig } from "../../../common/env";
import { StreamPreviewContext } from "../context/StreamPreviewContext";

export function StreamPathColumn(stream: Stream) {
  return (
    <Stack gap="1px">
      <Typography>{stream.streamPath}</Typography>
      <Typography color="neutral" fontSize="xs">
        Client Id: {stream.clientId}
      </Typography>
    </Stack>
  );
}

export function ResolutionColumn(stream: Stream) {
  return (
    <Typography>
      {stream.width} x {stream.height}
    </Typography>
  );
}

export function StartTimeColumn(stream: Stream) {
  const startTime = moment(stream.startTime);

  return (
    <Tooltip title={startTime.local().format("LLLL")} variant="solid">
      <Typography>{startTime.fromNow()}</Typography>
    </Tooltip>
  );
}

const envConfig = getEnvConfig();

export function StreamOptionsColumn(
  stream: Stream,
  context: IStreamTableContext
) {
  const { deleteStream, openPreview } = context;

  const closeStream = () => {
    deleteStream(stream.id);
  };

  const options: React.ReactNode[] = [];

  if (envConfig.HAS_HTTP_FLV_PREVIEW) {
    const uri = envConfig.HTTP_FLV_URI_PATTERN.replace(
      "{streamPath}",
      stream.streamPath
    );
    options.push(
      <MenuItem key="http-flv" onClick={() => openPreview(uri)}>
        Watch via HTTP-FLV
      </MenuItem>
    );
  }

  return (
    <Dropdown>
      <Box className="flex justify-end">
        <MenuButton
          slots={{ root: IconButton }}
          slotProps={{
            root: { variant: "plain", color: "neutral", size: "sm" },
          }}
        >
          <MoreHorizRoundedIcon />
        </MenuButton>
      </Box>
      <Menu size="sm" sx={{ minWidth: 140 }}>
        {options.length > 0 && options.map((x) => x)}
        {options.length > 0 && <Divider />}
        <MenuItem color="danger" onClick={closeStream}>
          Close
        </MenuItem>
      </Menu>
    </Dropdown>
  );
}
