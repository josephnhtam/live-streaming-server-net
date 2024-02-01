/* eslint-disable react-hooks/exhaustive-deps */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { Order, TableContext } from "../../../components/table";
import { useAppDispatch, useAppSelector } from "../../../store";
import {
  Stream,
  StreamsSelector,
  StreamsThunkActions,
} from "../../../store/features/streams";
import { StreamsTablePageSize } from "../../../common/constants";
import { StreamPreviewContext } from "./StreamPreviewContext";

interface ExtraStreamTableContext {
  filter: string;
  setFilter: (filter: string) => void;
  deleteStream: (streamId: string) => void;
  openPreview: (flvPreviewUri: string) => void;
}

export type IStreamTableContext = TableContext<Stream> &
  ExtraStreamTableContext;

export const StreamTableContext = createContext<IStreamTableContext>(null!);

const pageSize = StreamsTablePageSize;

export function useStreamTableContext() {
  const dispatch = useAppDispatch();
  const streamPreviewContext = useContext(StreamPreviewContext);

  const [order, setOrder] = useState<Order>("asc");
  const [orderBy, setOrderBy] = useState<keyof Stream | undefined>(undefined);
  const [selected, setSelected] = useState<Stream[]>([]);
  const [lastFilter, setLastFilter] = useState("");
  const [filter, setFilter] = useState("");

  const revision = useAppSelector((s) => s.streams.revision);
  const page = useAppSelector((s) => s.streams.page);
  const isLoading = useAppSelector((s) => s.streams.loading);
  const totalRows = useAppSelector((s) => s.streams.totalCount);
  const visibleRows = useAppSelector((s) =>
    StreamsSelector.selectAll(s.streams)
  );
  const totalPages = useAppSelector((s) =>
    Math.ceil(s.streams.totalCount / s.streams.pageSize)
  );

  const setPage = (page: number) => {
    dispatch(StreamsThunkActions.getStreams({ page, pageSize, filter }));
  };

  const refetch = useCallback(() => {
    dispatch(StreamsThunkActions.getStreams({ page, pageSize, filter }));
  }, [dispatch, page, filter]);

  const deleteStream = useCallback(
    (streamId: string) => {
      dispatch(StreamsThunkActions.deleteStream({ streamId }));
    },
    [dispatch]
  );

  const openPreview = (flvPreviewUri: string) => {
    streamPreviewContext.open(flvPreviewUri);
  };

  useEffect(() => refetch(), [revision]);

  useEffect(() => {
    if (lastFilter === filter) {
      return;
    }

    const timeout = setTimeout(() => {
      setLastFilter(filter);
      dispatch(StreamsThunkActions.getStreams({ page: 1, pageSize, filter }));
    }, 500);

    return () => clearTimeout(timeout);
  }, [dispatch, filter, lastFilter]);

  const value: IStreamTableContext = {
    order,
    setOrder,
    orderBy,
    setOrderBy,
    selected,
    setSelected,
    page,
    totalPages,
    setPage,
    isLoading,
    visibleRows,
    totalRows,
    refetch,
    filter,
    setFilter,
    deleteStream,
    openPreview,
  };

  return value;
}
