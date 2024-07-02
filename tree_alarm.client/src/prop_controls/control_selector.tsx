import * as React from 'react';
import { TextField } from '@mui/material';
import { MuiColorInput } from 'mui-color-input'
import { IControlSelector } from 'control_selector_common'
import renderGeoInput from './geo_input';


const renderTextField = (props: IControlSelector) => (
  <TextField
    size="small"
    fullWidth
    id={props.prop_name}
    label={props.prop_name}
    value={props.str_val}
    onChange={props.handleChangeProp}
  />
);

const renderColorInput = (props: IControlSelector) => {
  const handleChange = (newValue: string) => {
    // ׁמחהאול סמבûעטו, סמגלוסעטלמו ס handleChangeProp
    const event = {
      target: {
        id: props.prop_name,
        value: newValue
      }
    };
    props.handleChangeProp(event);
  };

  return (
    <MuiColorInput
      size="small"
      fullWidth
      format="hex"
      id={props.prop_name}
      label={props.prop_name}
      value={props.str_val}
      onChange={handleChange}
    />
  );
};



const controlMap: { [key: string]: (props: IControlSelector) => JSX.Element } = {
  __txt: renderTextField,
  __clr: renderColorInput,
  __geo: renderGeoInput,
  // Add other control types here
};
export function ControlSelector(props: IControlSelector) {

  const { visual_type } = props;
  const ControlComponent = controlMap[visual_type || '__txt'] || renderTextField;

  return (
    <> {ControlComponent(props)}</>
   
  );
}
