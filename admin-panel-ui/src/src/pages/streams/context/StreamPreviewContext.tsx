import { createContext, useState } from "react";

interface IStreamPreviewContext {
  previewInfo?: PreviewInfo;
  opened: boolean;
  open: (previewType: PreviewType, previewUri: string) => void;
  close: () => void;
}

export enum PreviewType {
  HttpFlv,
  Hls,
}

export interface PreviewInfo {
  previewType: PreviewType;
  previewUri: string;
}

export const StreamPreviewContext = createContext<IStreamPreviewContext>(null!);

export function useStreamPreviewContext() {
  const [opened, setOpened] = useState(false);
  const [previewInfo, setPreviewInfo] = useState<PreviewInfo | undefined>(
    undefined
  );

  const open = (previewType: PreviewType, previewUri: string) => {
    setPreviewInfo({
      previewType: previewType,
      previewUri: previewUri,
    });
    setOpened(true);
  };

  const close = () => {
    setOpened(false);
  };

  const value: IStreamPreviewContext = {
    previewInfo,
    opened,
    open,
    close,
  };

  return value;
}
