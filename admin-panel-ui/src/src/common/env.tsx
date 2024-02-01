interface EnvConfig {
  BASE_PATH: string;

  STREAMS_BASE_URI: string;

  HAS_HTTP_FLV_PREVIEW: boolean;
  HTTP_FLV_URI_PATTERN: string;
}

export const getEnvConfig = () => (window as any)._env as EnvConfig;
