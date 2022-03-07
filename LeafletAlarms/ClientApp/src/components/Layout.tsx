import 'bootstrap/dist/css/bootstrap.css';
import * as React from 'react';
import { Col, Container, Row } from 'reactstrap';
import NavMenu from './NavMenu';
import { TreeControl } from './TreeControl';

export default class Layout extends React.PureComponent<{}, { children?: React.ReactNode }> {
    public render() {
        return (
            <React.Fragment>
            <NavMenu />
              <Container>
              <Row>
                <Col sm="2"><div><TreeControl/></div></Col>
                <Col>
                  {this.props.children}
                </Col>
                <Col sm="1"><div> test</div></Col>
                </Row>
              </Container>
            </React.Fragment>
        );
    }
}