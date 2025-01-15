/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useState } from 'react';
import { List, ListItem, Button, TextField, Dialog, DialogActions, DialogContent, DialogTitle } from '@mui/material';
import { IActionDescrDTO, IActionParameterDTO, IActionExeDTO } from '../store/Marker';
import { useAppDispatch } from '../store/configureStore';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import * as ActionsStore from '../store/ActionsStates';


export function ActionsControl() {

  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const actions = useSelector((state: ApplicationState) => state?.actionsStates?.actions) ?? [];

  const [selectedAction, setSelectedAction] = useState<IActionDescrDTO | null>(null);
  const [parameters, setParameters] = useState<IActionParameterDTO[]>([]);
  const [numAction, setNumAction] = useState<number>(0);

  useEffect(() => {
    if (selected_id)
      appDispatch(ActionsStore.fetchAvailableActions(selected_id));
  }, [selected_id, numAction]);


  const executeAction = async () => {
    if (!selectedAction || !selected_id) return;
    const payload: IActionExeDTO = {
      object_id: selected_id,
      name: selectedAction.name!,
      parameters,
    };
    appDispatch(ActionsStore.executeAction(payload));
    setSelectedAction(null);
    setNumAction(numAction+1); // Обновляем список после выполнения
  };

  return (
    <div>
      <List>
        {actions.map((action) => (
          <ListItem key={action.name}>
            <Button onClick={() => {
              setSelectedAction(action);
              setParameters(action.parameters.map(p => ({ ...p })));
            }}>
              {action.name}
            </Button>
          </ListItem>
        ))}
      </List>

      <Dialog open={!!selectedAction} onClose={() => setSelectedAction(null)}>
        <DialogTitle>Execute Action: {selectedAction?.name}</DialogTitle>
        <DialogContent>
          {parameters.map((param, index) => (
            <TextField
              key={param.name}
              label={param.name}
              value={param.cur_val || ''}
              onChange={(e) =>
                setParameters((prev) =>
                  prev.map((p, i) => (i === index ? { ...p, cur_val: e.target.value } : p))
                )
              }
              fullWidth
              margin="dense"
            />
          ))}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelectedAction(null)}>Cancel</Button>
          <Button onClick={executeAction} color="primary">
            Execute
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}

