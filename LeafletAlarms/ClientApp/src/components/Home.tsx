import * as React from 'react';
import { connect } from 'react-redux';
import { MapComponent } from '../map/MapComponent'

const Home = () => (
  <div>
    <h1>Goodbye, world!</h1>
    <MapComponent/>
  </div>
);

export default connect()(Home);
