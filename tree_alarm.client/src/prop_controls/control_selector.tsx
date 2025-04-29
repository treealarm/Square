import * as React from 'react';
import { TextField } from '@mui/material';
import { MuiColorInput } from 'mui-color-input'
import { IControlSelector } from './control_selector_common'
import { renderPasswordField } from './RenderPasswordField'
import { VisualTypes } from '../store/Marker';

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
    // Создаем событие, совместимое с handleChangeProp
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



// eslint-disable-next-line no-unused-vars
const controlMap: { [key: string]: (props_in: IControlSelector) => React.ReactElement } = {
  [VisualTypes.Text]: (props_in) => renderTextField(props_in),
  [VisualTypes.Color]: (props_in) => renderColorInput(props_in),
  [VisualTypes.Password]: (props_in) => renderPasswordField(props_in),
  // другие типы
};


export function ControlSelector(props: IControlSelector) {

  const { visual_type } = props;
  const ControlComponent = controlMap[visual_type || VisualTypes.Text] || renderTextField;

  return (
    <> {ControlComponent(props)}</>
   
  );
}
