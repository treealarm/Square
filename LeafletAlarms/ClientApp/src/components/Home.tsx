import * as React from "react";
import { connect } from "react-redux";
import { MapComponent } from "../map/MapComponent";

export function Home() {
  return (
    <div>
      <h1>Goodbye, world!</h1>
      <MapComponent />
    </div>
  );
}

