import * as React from 'react';
import Drawer from '@mui/material/Drawer';
import Button from '@mui/material/Button';
import { FormControl, FormControlLabel, FormLabel, Radio, RadioGroup } from '@mui/material';
import * as EditStore from '../store/EditStates';
import { useDispatch, useSelector } from 'react-redux';

export default function EditOptions() {

  const dispatch = useDispatch();
  const selected = useSelector((state) => state?.editState?.figure);

  const [state, setState] = React.useState(false);

  const toggleDrawer = (open: boolean) => (event: any) => {
    if (event.type === 'keydown' && (event.key === 'Tab' || event.key === 'Shift')) {
      return;
    }

    setState(open);
  };

  function valueChanged(event: React.ChangeEvent<HTMLInputElement>, value: string) {
    const val: EditStore.Figures = value as unknown as EditStore.Figures;
    dispatch(EditStore.actionCreators.setFigure(val));
  };

  const anchor = 'left';

  return (
    <div>
      
      <React.Fragment key={anchor}>
        <Button onClick={toggleDrawer(true)} style={{ textTransform: 'none' }}>
          Tool:{EditStore.Figures[selected]}
        </Button>
          <Drawer
            anchor={anchor}
            open={state}
            onClose={toggleDrawer(false)}
          >
          <FormControl>
            <FormLabel id="options">Select tool</FormLabel>
            <RadioGroup
              aria-labelledby="options-group-label"
              defaultValue={selected}
              name="radio-buttons-group"
              onChange={valueChanged}
            >
              {
                Object.entries(EditStore.Figures).map((item) =>
                  <FormControlLabel value={item[0]} control={<Radio />} label={item[1]} />
                  )
              }
            </RadioGroup>
          </FormControl>
          </Drawer>
        </React.Fragment>
    </div>
  );
}