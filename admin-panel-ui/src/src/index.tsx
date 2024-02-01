import * as React from "react";
import * as ReactDOM from "react-dom/client";
import {
  CssVarsProvider as JoyCssVarsProvider,
  StyledEngineProvider,
} from "@mui/joy/styles";
import {
  experimental_extendTheme as materialExtendTheme,
  Experimental_CssVarsProvider as MaterialCssVarsProvider,
  THEME_ID as MATERIAL_THEME_ID,
} from "@mui/material/styles";
import { ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import theme from "./theme";
import App from "./App";
import "./index.css";

const materialTheme = materialExtendTheme();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <ThemeProvider theme={theme}>
    <StyledEngineProvider injectFirst>
      <MaterialCssVarsProvider theme={{ [MATERIAL_THEME_ID]: materialTheme }}>
        <JoyCssVarsProvider>
          <CssBaseline />
          <App />
        </JoyCssVarsProvider>
      </MaterialCssVarsProvider>
    </StyledEngineProvider>
  </ThemeProvider>
);
