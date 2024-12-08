import { Box, Modal, ModalClose, Sheet, Typography } from "@mui/joy";
import { useCallback, useContext, useEffect, useState } from "react";
import {
  PreviewInfo,
  PreviewType,
  StreamPreviewContext,
} from "../context/StreamPreviewContext";
import flvjs from "flv.js";
import Hls from "hls.js";

export default function StreamPreview() {
  const { previewInfo, opened, close } = useContext(StreamPreviewContext);
  const [video, setVideo] = useState<HTMLVideoElement | null>(null);
  const refCallback = useCallback((ref: HTMLVideoElement) => setVideo(ref), []);

  useVideoMounting(opened, video, previewInfo);

  return (
    <>
      <Modal
        aria-labelledby="modal-title"
        aria-describedby="modal-desc"
        open={opened}
        onClose={close}
        className="flex justify-center items-center"
      >
        <Sheet
          variant="outlined"
          className="max-w-[80%] p-6 rounded-lg shadow-lg"
        >
          <ModalClose variant="plain" sx={{ m: 1 }} />
          <Typography
            component="h2"
            id="modal-title"
            level="h4"
            textColor="inherit"
            fontWeight="lg"
            mb={1}
          >
            Stream Preview
          </Typography>
          <Box className="w-[1280px] max-w-[70vw] aspect-video flex justify-center">
            <video
              className="w-full h-full"
              controls={false}
              ref={refCallback}
            />
          </Box>
        </Sheet>
      </Modal>
    </>
  );
}

function useVideoMounting(
  opened: boolean,
  video: HTMLVideoElement | null,
  previewInfo?: PreviewInfo
) {
  useEffect(() => {
    if (!opened || !video || !previewInfo) {
      return;
    }

    switch (previewInfo.previewType) {
      case PreviewType.HttpFlv: {
        const flvPlayer = mountFlvPlayer(previewInfo.previewUri, video);
        return () => flvPlayer?.destroy();
      }
      case PreviewType.Hls: {
        const hlsPlayer = mountHlsPlayer(previewInfo.previewUri, video);
        return () => hlsPlayer?.destroy();
      }
    }
  }, [opened, video, previewInfo]);
}

function mountFlvPlayer(previewUri: string, video: HTMLVideoElement) {
  if (!flvjs.isSupported) return null;

  const flvPlayer = flvjs.createPlayer({
    type: "flv",
    url: previewUri,
    isLive: true,
    cors: true,
  });

  flvPlayer.attachMediaElement(video);
  flvPlayer.load();
  flvPlayer.play();

  return flvPlayer;
}

function mountHlsPlayer(previewUri: string, video: HTMLVideoElement) {
  if (!Hls.isSupported()) return null;

  const hls = new Hls({
    enableWorker: true,
    lowLatencyMode: true,
    liveSyncDurationCount: 4,
    liveMaxLatencyDurationCount: 8,
  });

  hls.attachMedia(video);

  hls.on(Hls.Events.MEDIA_ATTACHED, (event, data) => {
    hls.loadSource(previewUri);
    video.play();
  });

  return hls;
}
