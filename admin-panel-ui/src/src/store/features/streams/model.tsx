export interface Stream {
  id: string;
  clientId: number;
  streamPath: string;
  videoCodecId: number;
  height: number;
  width: number;
  framerate: number;
  audioCodecId: number;
  audioSampleRate: number;
  audioChannels: number;
  subscribersCount: number;
  startTime: string;
  streamArguments: { [key: string]: string };
}
