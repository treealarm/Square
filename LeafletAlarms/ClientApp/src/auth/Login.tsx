import * as React from "react";
import { Button, Paper, styled, Tooltip } from "@mui/material";
import { useEffect } from "react";
import { useSelector } from "react-redux";
import UserService from "./UserService";
import { ApplicationState } from "../store";


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
      
        <Tooltip title={UserService.getToken()}>
          <Button
            variant="contained"
            onClick={onButtonClick}
            style={{ textTransform: 'none' }}
            size="small"
          >{!loggedIn ? "login" : "logout " + user}
          </Button>
        </Tooltip>
      
    </React.Fragment>
  );
}