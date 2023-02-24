import { FormControl, InputLabel, MenuItem, Select, SelectChangeEvent } from "@mui/material";



export function GroupSelector(props: any) {

  function handleChange(event: SelectChangeEvent) {
    const { target: { value } } = event;
    props.onChange(props.id, value);
  };

  return (

    <FormControl fullWidth>
      <InputLabel id="group-select-label">{props.label}</InputLabel>
      <Select
        labelId="select_group"
        id={props.id}
        value={props.value}
        label="group id"
        onChange={handleChange}
        size="small"
      >
        {
          props.options.map((item: string) =>
            <MenuItem value={item}>{item}</MenuItem>
            )
        }
      </Select>
    </FormControl>

    );
}