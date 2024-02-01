import * as React from "react";
import Box from "@mui/material/Box";
import JoyTable from "@mui/joy/Table";
import { iconButtonClasses } from "@mui/joy/IconButton";
import { Button, Sheet, Typography } from "@mui/joy";
import KeyboardArrowRightIcon from "@mui/icons-material/KeyboardArrowRight";
import KeyboardArrowLeftIcon from "@mui/icons-material/KeyboardArrowLeft";
import Pagination from "@mui/material/Pagination";
import Checkbox from "@mui/joy/Checkbox";
import { Entity, TableContext, TableProps } from "./model";
import TableHead from "./TableHead";

export default React.memo(Table) as typeof Table;

function Table<Data extends Entity, Context extends TableContext<Data>>(
  props: TableProps<Data, Context>
) {
  const { headCells, isSelectable, context, sx, tableSx } = props;

  const tableContext = React.useContext(context);

  const {
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
  } = tableContext;

  const handleRequestSort = (id: keyof Data) => {
    const isAsc = orderBy === id && order === "asc";
    setOrder(isAsc ? "desc" : "asc");
    setOrderBy(id);
  };

  const handleSelectAllClick = () => {
    let newSelected = selected.filter(
      (x) => !visibleRows.find((y) => y.id === x.id)
    );

    if (!visibleRows.every((x) => selected.find((y) => y.id === x.id))) {
      newSelected = [...newSelected, ...visibleRows];
      setSelected(newSelected);
    } else {
      setSelected(newSelected);
    }
  };

  const handleClick = (data: Data) => {
    const selectedIndex = selected.findIndex((x) => x.id === data.id);
    let newSelected: Data[] = [];

    if (selectedIndex === -1) {
      newSelected = newSelected.concat(selected, data);
    } else if (selectedIndex === 0) {
      newSelected = newSelected.concat(selected.slice(1));
    } else if (selectedIndex === selected.length - 1) {
      newSelected = newSelected.concat(selected.slice(0, -1));
    } else if (selectedIndex > 0) {
      newSelected = newSelected.concat(
        selected.slice(0, selectedIndex),
        selected.slice(selectedIndex + 1)
      );
    }
    setSelected(newSelected);
  };

  const handleChangePage = (newPage: number) => {
    setPage(newPage);
  };

  const isSelected = (id: string) =>
    selected.findIndex((x) => x.id === id) !== -1;

  return (
    <>
      <Sheet
        className="OrderTableContainer"
        variant="outlined"
        sx={{
          width: "100%",
          borderRadius: "sm",
          flexShrink: 1,
          overflow: "auto",
          minHeight: 0,
          ...sx,
        }}
      >
        <JoyTable
          aria-labelledby="tableTitle"
          stickyHeader
          hoverRow
          sx={{
            "--TableCell-headBackground":
              "var(--joy-palette-background-level1)",
            "--Table-headerUnderlineThickness": "1px",
            "--TableRow-hoverBackground":
              "var(--joy-palette-background-level1)",
            "--TableCell-paddingY": "4px",
            "--TableCell-paddingX": "8px",
            ...tableSx,
          }}
        >
          <TableHead
            order={order}
            orderBy={orderBy as string}
            onSelectAllClick={handleSelectAllClick}
            onRequestSort={handleRequestSort}
            headCells={headCells}
            numSelected={selected.length}
            totalRows={totalRows}
            isLoading={isLoading}
            isSelectable={isSelectable}
          />

          <tbody>
            {visibleRows.map((row, index) => {
              const isItemSelected = isSelected(row.id);

              return (
                <tr key={row.id}>
                  {isSelectable ? (
                    <td
                      style={{
                        textAlign: "center",
                        width: 120,
                        overflow: "hidden",
                      }}
                    >
                      <Checkbox
                        size="sm"
                        checked={isItemSelected}
                        color={isItemSelected ? "primary" : undefined}
                        onChange={() => handleClick(row)}
                        slotProps={{ checkbox: { sx: { textAlign: "left" } } }}
                        sx={{ verticalAlign: "text-bottom" }}
                      />
                    </td>
                  ) : (
                    <td />
                  )}

                  {headCells.map((cell, index) => {
                    const key = cell.id;
                    let data = row[key] as any;
                    const elKey = `${row.id}-${index}`;

                    const paddingRight =
                      index === headCells.length - 1 ? "16px" : 0;

                    if (cell.customNode) {
                      return (
                        <td
                          key={elKey}
                          style={{ paddingRight, overflow: "hidden" }}
                        >
                          {cell.customNode(row, tableContext)}
                        </td>
                      );
                    }

                    return (
                      <td key={elKey} style={{ paddingRight }}>
                        <Typography level="body-xs">{data}</Typography>
                      </td>
                    );
                  })}
                </tr>
              );
            })}
          </tbody>
        </JoyTable>
      </Sheet>

      <Box
        className="Pagination-laptopUp"
        sx={{
          pt: 2,
          gap: 1,
          [`& .${iconButtonClasses.root}`]: { borderRadius: "50%" },
          display: "flex",
        }}
      >
        <Button
          size="sm"
          variant="outlined"
          color="neutral"
          startDecorator={<KeyboardArrowLeftIcon />}
          disabled={page <= 1}
          onClick={() => setPage(page - 1)}
        >
          Previous
        </Button>

        <Box sx={{ flex: 1 }} />
        <Pagination
          page={page}
          count={totalPages}
          variant="outlined"
          hidePrevButton={true}
          hideNextButton={true}
          onChange={(_, page) => setPage(page)}
        />
        <Box sx={{ flex: 1 }} />

        <Button
          size="sm"
          variant="outlined"
          color="neutral"
          endDecorator={<KeyboardArrowRightIcon />}
          disabled={page >= totalPages}
          onClick={() => setPage(page + 1)}
        >
          Next
        </Button>
      </Box>
    </>
  );
}
