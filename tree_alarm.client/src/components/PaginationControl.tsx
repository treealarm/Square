/* eslint-disable no-unused-vars */
import { useState } from "react";
import { ButtonGroup, IconButton, ToggleButton, Tooltip, Typography } from "@mui/material";
import FirstPageIcon from "@mui/icons-material/FirstPage";
import NavigateBeforeIcon from "@mui/icons-material/NavigateBefore";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";

const PaginationControl = ({ autoUpdate, OnNavigate }: { autoUpdate: boolean; OnNavigate: (page: number) => void }) => {
  const [page, setPage] = useState(0);

  const goToFirstPage = () => {
    setPage(0);
    OnNavigate(0);
  };

  const goToPreviousPage = () => {
    setPage((prev) => Math.max(0, prev - 1));
    OnNavigate(Math.max(0, page - 1));
  };

  const goToNextPage = () => {
    setPage((prev) => prev + 1);
    OnNavigate(page + 1);
  };

  return (
    <ButtonGroup>
      <Tooltip title={"First events page " + (!autoUpdate ? "/ autoupdate on" : "/ autoupdate off")}>
        <ToggleButton sx={{ borderRadius: 1, border: 0 }} value="check" selected={autoUpdate} onClick={goToFirstPage}>
          <FirstPageIcon />
        </ToggleButton>
      </Tooltip>
      <Tooltip title="Previous events page">
        <IconButton sx={{ borderRadius: 1, border: 0 }} onClick={goToPreviousPage} disabled={page === 0}>
          <NavigateBeforeIcon />
        </IconButton>
      </Tooltip>
      <Typography
        sx={{
          padding: "6px 12px",
          minWidth: "40px",
          textAlign: "center",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        {page}
      </Typography>

      <Tooltip title="Next events page">
        <IconButton sx={{ borderRadius: 1, border: 0 }} onClick={goToNextPage}>
          <NavigateNextIcon />
        </IconButton>
      </Tooltip>
    </ButtonGroup>
  );
};

export default PaginationControl;
