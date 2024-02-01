import { Sheet } from "@mui/joy";
import React from "react";

export default function Main(props: Props) {
  const { scrollable, children } = props;

  return (
    <Sheet
      component="main"
      className="MainContent"
      sx={{
        px: { xs: 2, md: 6 },
        pt: {
          xs: "calc(12px + var(--Header-height))",
          sm: "calc(12px + var(--Header-height))",
          md: 3,
        },
        pb: { xs: 2, sm: 2, md: 3 },
        flex: 1,
        minWidth: 0,
        height: scrollable ? "inherit" : "100dvh",
        overflowY: "hidden",
      }}
    >
      {children}
    </Sheet>
  );
}

interface Props {
  scrollable?: boolean;
  children: React.ReactNode;
}
