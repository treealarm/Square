import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router';
import { ApplicationState } from '../store';
import * as MarkersStore from '../store/MarkersStates';

// At runtime, Redux will merge together...
type MarkersProps =
  MarkersStore.MarkersState // ... state we've requested from the Redux store
  & typeof MarkersStore.actionCreators // ... plus action creators we've requested
  & RouteComponentProps<{ box: string }>; // ... plus incoming routing parameters


class FetchMarkers extends React.PureComponent<MarkersProps> {
  // This method is called when the component is first added to the document
  public componentDidMount() {
    this.ensureDataFetched();
  }

  // This method is called when the route parameters change
  public componentDidUpdate() {
    this.ensureDataFetched();
  }

  public render() {
    return (
      <React.Fragment>
        <h1 id="tabelLabelMarkers">Markers</h1>
        <p>This component demonstrates fetching data from the server and working with URL parameters.</p>
        {this.renderMarkersTable()}
      </React.Fragment>
    );
  }

  private ensureDataFetched() {
    const box = this.props.match.params.box;
    this.props.requestMarkers(box);
  }

  private renderMarkersTable() {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabelMarkers">
        <thead>
          <tr>
            <th>Id</th>
            <th>Name</th>
            <th>Points</th>
          </tr>
        </thead>
        <tbody>
          {this.props.isLoading && <span>Loading...</span>}
          {this.props.markers.map((marker: MarkersStore.Marker) =>
            <tr key={marker.id}>
              <td>{marker?.id}</td>
              <td>{marker?.name}</td>
              <td>{marker?.points}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }
/////
}

export default connect(
  (state: ApplicationState) => state.markersStates, // Selects which state properties are merged into the component's props
  MarkersStore.actionCreators // Selects which action creators are merged into the component's props
)(FetchMarkers as any); // eslint-disable-line @typescript-eslint/no-explicit-any
