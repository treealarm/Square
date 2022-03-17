import "bootstrap/dist/css/bootstrap.css";
import * as React from "react";
import { Container } from "reactstrap";
import NavMenu from "./NavMenu";

export default class Layout extends React.PureComponent<
  {},
  { children?: React.ReactNode }
> {
  public render() {
    return (
      <React.Fragment>
        <NavMenu />
        <Container fluid>{this.props.children}</Container>
      </React.Fragment>
    );
  }
}
