import * as React from 'react';
import Drawer from '@mui/material/Drawer';
import Button from '@mui/material/Button';
import { FormControl, FormControlLabel, FormLabel, Radio, RadioGroup } from '@mui/material';

export default function EditOptions() {

  const [selected, setSelected] = React.useState('circle');
  const [state, setState] = React.useState(false);

  const toggleDrawer = (open) => (event) => {
    if (event.type === 'keydown' && (event.key === 'Tab' || event.key === 'Shift')) {
      return;
    }

    setState(open);
  };

  function valueChanged (event: React.ChangeEvent<HTMLInputElement>, value: string){
    setSelected(value);
  };

  const anchor = 'left';

  return (
    <div>
      
      <React.Fragment key={anchor}>
        <Button onClick={toggleDrawer(true)} style={{ textTransform: 'none' }}>Tool:{selected}</Button>
          <Drawer
            anchor={anchor}
            open={state}
            onClose={toggleDrawer(false)}
          >
          <FormControl>
            <FormLabel id="options">Gender</FormLabel>
            <RadioGroup
              aria-labelledby="options-group-label"
              defaultValue={selected}
              name="radio-buttons-group"
              onChange={valueChanged}
            >
              <FormControlLabel value="circle" control={<Radio />} label="Circle" />
              <FormControlLabel value="poligon" control={<Radio />} label="Poligon" />
              <FormControlLabel value="poliline" control={<Radio />} label="Poliline" />
            </RadioGroup>
          </FormControl>
          </Drawer>
        </React.Fragment>
    </div>
  );
}