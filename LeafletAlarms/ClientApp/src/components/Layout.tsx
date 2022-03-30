import * as React from "react";

export default class Layout extends React.PureComponent<
  {},
  { children?: React.ReactNode }
> {
  public render() {
    return <React.Fragment>{this.props.children}</React.Fragment>;
  }
}
