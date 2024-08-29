import GeoEditor from './geo_editor';
import { IControlSelector } from './control_selector_common';

const renderGeoInput = (props: IControlSelector) => (
  <GeoEditor props={props} />
);

export default renderGeoInput;
