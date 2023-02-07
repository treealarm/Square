import * as React from "react";
import { Box, Button, IconButton, Paper, styled, Tooltip } from "@mui/material";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import UserService from "./UserService";

const Item = styled(Paper)(({ theme }) => ({
  ...theme.typography.body2,
  textAlign: 'center',
  color: theme.palette.text.secondary,
  height: 10,
  lineHeight: '10px',
}));

export function Login() {

  const dispatch = useDispatch();
  
  useEffect(
    () => {

    }, []);

  React.useMemo(() => {
    
  }, []);

  const onButtonClick = (event: React.MouseEvent<HTMLElement>) => {  

    if (!UserService.isLoggedIn()) {
      UserService.doLogin();
    }
    else {
      UserService.doLogout();
    }
  };

  return (
    <React.Fragment key={"Login"}>
      <Box sx={{ border: 1 }}>
        <Tooltip title={UserService.getToken()}>
          <Button
            onClick={onButtonClick}
            style={{ textTransform: 'none' }}
            size="small"
          >{!UserService.isLoggedIn() ? "Login" : "Logout " + UserService.getUsername()}
          </Button>
        </Tooltip>

      </Box>

    </React.Fragment>
  );
}