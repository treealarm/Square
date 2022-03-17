import * as React from "react";
import { Col, Container, Row } from "reactstrap";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";

export function Home() {
  return (
    <Container fluid>
      <Row>
        <TabControl />
      </Row>
      <Row>
        <Col sm="2">
          <div>
            <TreeControl />
          </div>
        </Col>
        <Col>
          <div>
            <h1>Goodbye, world!</h1>
            <MapComponent />
          </div>
        </Col>
        <Col sm="1">
          <div> test</div>
        </Col>
      </Row>
    </Container>
  );
}
