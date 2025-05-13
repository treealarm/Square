/* eslint-disable no-unused-vars */
import CoordInput from "../prop_controls/CoordInput";

interface Props {
  lat: number;
  lon: number;
  onChange: (lat: number, lon: number) => void;
}

export const CoordinatesInput = (props: Props) => (
  <CoordInput
    lat={props.lat}
    lng={props.lon}
    index={0}
    onCoordChange={(_, newLat, newLon) => props.onChange(newLat, newLon)}
  />
);
