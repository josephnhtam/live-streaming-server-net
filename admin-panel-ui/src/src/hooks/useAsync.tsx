import { useCallback, useState } from "react";

interface State<TResult> {
  loading: boolean;
  succeeded: boolean;
  hasError: boolean;
  result?: TResult;
  error?: any;
}

export default function useAsync<TResult>() {
  const [state, setState] = useState<State<TResult>>({
    loading: false,
    succeeded: false,
    hasError: false,
  });

  const reset = useCallback(() => {
    setState({
      loading: false,
      succeeded: false,
      hasError: false,
    });
  }, [setState]);

  const exec = useCallback(
    (func: () => Promise<TResult>) => {
      const doExec = async () => {
        try {
          const result = await func();
          setState({
            loading: false,
            succeeded: true,
            hasError: false,
            result,
          });
        } catch (error) {
          setState({
            loading: false,
            succeeded: false,
            hasError: true,
            error,
          });
        }
      };

      setState({
        loading: true,
        succeeded: false,
        hasError: false,
      });

      doExec();
    },
    [setState]
  );

  return {
    loading: state.loading,
    succeeded: state.succeeded,
    hasError: state.hasError,
    result: state.result,
    error: state.error,
    exec,
    reset,
  };
}
