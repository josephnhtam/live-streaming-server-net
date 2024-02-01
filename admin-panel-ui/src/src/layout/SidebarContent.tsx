import * as React from "react";
import Box from "@mui/joy/Box";
import List from "@mui/joy/List";
import ListItem from "@mui/joy/ListItem";
import ListItemButton from "@mui/joy/ListItemButton";
import ListItemContent from "@mui/joy/ListItemContent";
import Typography from "@mui/joy/Typography";
import ColorSchemeToggle from "./ColorSchemeToggle";
import ConnectedTvIcon from "@mui/icons-material/ConnectedTv";
import { useNavigate } from "react-router-dom";

function SidebarContent() {
  const navigate = useNavigate();

  return (
    <>
      <List
        size="sm"
        sx={{
          gap: 1,
          "--ListItem-radius": (theme) => theme.vars.radius.sm,
        }}
      >
        <ListItem>
          <ListItemButton onClick={() => navigate("streams")}>
            <ConnectedTvIcon />
            <ListItemContent>
              <Typography level="title-sm">Streams</Typography>
            </ListItemContent>
          </ListItemButton>
        </ListItem>
      </List>

      <Box className="flex items-center">
        <ColorSchemeToggle sx={{ mr: "auto" }} />
      </Box>
    </>
  );
}

export default React.memo(SidebarContent);
