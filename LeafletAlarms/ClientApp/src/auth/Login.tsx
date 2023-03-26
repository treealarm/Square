import * as React from "react";
import { Box, Button, IconButton, Paper, styled, Tooltip } from "@mui/material";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import UserService from "./UserService";
import { ApplicationState } from "../store";

const Item = styled(Paper)(({ theme }) => ({
  ...theme.typography.body2,
  textAlign: 'center',
  color: theme.palette.text.secondary,
  height: 10,
  lineHeight: '10px',
}));

export function Login() {
  const user = useSelector((state: ApplicationState) => state?.rightsStates?.user);

  const [loggedIn, setLoggedIn] = React.useState(UserService.isLoggedIn());

  useEffect(
    () => {
      setLoggedIn(UserService.isLoggedIn());
    }, [user]);

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
          >{!loggedIn ? "login" : "logout " + user}
          </Button>
        </Tooltip>
      </Box>
    </React.Fragment>
  );
}