import { Box, Modal, ModalClose, Sheet, Typography } from "@mui/joy";
import { useCallback, useContext, useEffect, useState } from "react";
import { StreamPreviewContext } from "../context/StreamPreviewContext";
import flvjs from "flv.js";

export default function StreamPreview() {
  const { flvPreviewUri, opened, close } = useContext(StreamPreviewContext);
  const [video, setVideo] = useState<HTMLVideoElement | null>(null);
  const [flvPlayer, setFlvPlayer] = useState<flvjs.Player | null>(null);

  const refCallback = useCallback((ref: HTMLVideoElement) => {
    setVideo(ref);
  }, []);

  useEffect(() => {
    if (!video || !flvjs.isSupported) return;

    const flvPlayer = flvjs.createPlayer({
      type: "flv",
      url: flvPreviewUri,
      isLive: true,
      cors: true,
    });

    flvPlayer.attachMediaElement(video);
    flvPlayer.load();
    flvPlayer.play();

    setFlvPlayer(flvPlayer);
  }, [video, flvPreviewUri]);

  useEffect(() => {
    if (opened || !flvPlayer) return;

    flvPlayer.destroy();
  }, [flvPlayer, opened]);

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
              ref={refCallback}
              controls={false}
            />
          </Box>
        </Sheet>
      </Modal>
    </>
  );
}
