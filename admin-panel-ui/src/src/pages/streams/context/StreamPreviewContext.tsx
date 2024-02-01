import { createContext, useState } from "react";

interface IStreamPreviewContext {
  flvPreviewUri?: string;
  opened: boolean;
  open: (flvPreviewUri: string) => void;
  close: () => void;
}

export const StreamPreviewContext = createContext<IStreamPreviewContext>(null!);

export function useStreamPreviewContext() {
  const [opened, setOpened] = useState(false);
  const [flvPreviewUri, setFlvPreviewUri] = useState<string | undefined>(
    undefined
  );

  const open = (flvPreviewUri: string) => {
    setFlvPreviewUri(flvPreviewUri);
    setOpened(true);
  };

  const close = () => {
    setOpened(false);
  };

  const value: IStreamPreviewContext = {
    flvPreviewUri,
    opened,
    open,
    close,
  };

  return value;
}
