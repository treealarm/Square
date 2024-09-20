import GeoEditor from './geo_editor';
import { IControlSelector, IControlGeoProps } from './control_selector_common';

const renderGeoInput = (props: IControlSelector) =>
{
  const geo_props: IControlGeoProps =
  {
    val: JSON.parse(props.str_val),
    handleChangeProp: props.handleChangeProp,
    prop_name: props.prop_name
  }

  return (
    < GeoEditor props={geo_props} />
  );
};

export default renderGeoInput;
