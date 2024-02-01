import * as React from "react";
import Box from "@mui/material/Box";
import { Link } from "@mui/joy";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import Checkbox from "@mui/joy/Checkbox";
import { TableContext, TableHeadProps } from "./model";

export default TableHead as typeof TableHead;

function TableHead<Data, Context extends TableContext<Data>>(
  props: TableHeadProps<Data, Context>
) {
  const {
    headCells,
    order,
    orderBy,
    onRequestSort,
    onSelectAllClick,
    numSelected,
    totalRows,
    isLoading,
    isSelectable,
  } = props;

  return (
    <thead>
      <tr>
        {isSelectable ? (
          <th style={{ width: 48, textAlign: "center", padding: "12px 6px" }}>
            <Checkbox
              size="sm"
              indeterminate={numSelected > 0 && numSelected < totalRows}
              checked={numSelected >= totalRows}
              onChange={onSelectAllClick}
              color={
                numSelected > 0 || numSelected >= totalRows
                  ? "primary"
                  : undefined
              }
              sx={{ verticalAlign: "text-bottom" }}
            />
          </th>
        ) : (
          <th className="SelectablePlaceholder" style={{ width: 16 }} />
        )}

        {headCells.map((headCell, index) => (
          <th
            key={headCell.id as string}
            style={{
              width: headCell.width,
              padding: "12px 6px",
            }}
          >
            <Box
              sx={{
                display: "flex",
                justifyContent:
                  headCell.align === "right" ? "flex-end" : "flex-start",
                paddingRight: index === headCells.length - 1 ? "16px" : 0,
              }}
            >
              {headCell.sortable ? (
                <Link
                  underline="none"
                  color="primary"
                  component="button"
                  onClick={() => onRequestSort(headCell.id)}
                  fontWeight="lg"
                  endDecorator={
                    <ArrowDropDownIcon
                      sx={{
                        opacity: orderBy !== headCell.id ? 0 : 1,
                      }}
                    />
                  }
                  sx={{
                    "& svg": {
                      transform:
                        order === "desc" ? "rotate(0deg)" : "rotate(180deg)",
                    },
                  }}
                >
                  {headCell.label}
                </Link>
              ) : (
                <>{headCell.label}</>
              )}
            </Box>
          </th>
        ))}
      </tr>
    </thead>
  );
}
