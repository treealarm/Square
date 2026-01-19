/* eslint-disable no-unused-vars */
import * as React from "react";
import { Button, Tooltip } from "@mui/material";
import { useAppSelector, useAppDispatch } from "../store/configureStore";
import { logout } from "../store/authSlice";
import { useNavigate } from "react-router-dom";

export function LoginButton() {
  const dispatch = useAppDispatch();
  const token = useAppSelector((s) => s.authStates.token);
  const username = "";// useAppSelector((s) => s.authStates.username);
  const navigate = useNavigate();

  const handleClick = () => {
    if (!token) {
      navigate("/login");
    } else {
      dispatch(logout());
    }
  };

  return (
    <Tooltip title={username || ""}>
      <Button
        variant="contained"
        size="small"
        onClick={handleClick}
        style={{ textTransform: "none" }}
      >
        {!token ? "Login" : `Logout ${username}`}
      </Button>
    </Tooltip>
  );
}
