import { EffectCallback, useEffect, useState } from "react";

export default function useInit(func: EffectCallback) {
  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    if (initialized) return;

    setInitialized(true);
    return func();
  }, [initialized, func]);
}
