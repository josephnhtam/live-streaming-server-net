import { SxProps } from "@mui/joy/styles/types";

export type Order = "asc" | "desc";
export type Align = "left" | "right";

export interface Entity {
  id: string;
}

export interface HeadCell<Data, Context extends TableContext<Data>> {
  id: keyof Data;
  label: string;
  align?: Align;
  sortable?: boolean;
  width?: string;
  customNode?: (rawData: Data, context: Context) => React.ReactNode;
}

export interface TableProps<Data, Context extends TableContext<Data>> {
  headCells: HeadCell<Data, Context>[];
  context: React.Context<Context>;
  isSelectable?: boolean;
  sx?: SxProps;
  tableSx?: SxProps;
}

export interface TableContext<Data> {
  order: Order;
  setOrder: (order: Order) => void;

  orderBy?: keyof Data;
  setOrderBy: (orderBy?: keyof Data) => void;

  selected: Data[];
  setSelected: (selected: Data[]) => void;

  page: number;
  setPage: (page: number) => void;

  totalPages: number;

  isLoading: boolean;
  visibleRows: Data[];
  totalRows: number;

  refetch: () => void;
}

export interface TableHeadProps<Data, Context extends TableContext<Data>> {
  onRequestSort: (id: keyof Data) => void;
  onSelectAllClick: () => void;
  order: Order;
  orderBy: string;
  headCells: HeadCell<Data, Context>[];
  numSelected: number;
  totalRows: number;
  isLoading: boolean;
  isSelectable?: boolean;
}
