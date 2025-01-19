/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect, useState } from 'react';
import {
  List,
  ListItem,
  Button,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  TextField,
  Box,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { IActionDescrDTO, IActionParameterDTO, IActionExeDTO } from '../store/Marker';
import { useAppDispatch } from '../store/configureStore';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import * as ActionsStore from '../store/ActionsStates';

export function ActionsControl() {
  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const actions = useSelector((state: ApplicationState) => state?.actionsStates?.actions) ?? [];

  const [expandedAction, setExpandedAction] = useState<string | null>(null);
  const [actionParameters, setActionParameters] = useState<Record<string, IActionParameterDTO[]>>({});
  const [numAction, setNumAction] = useState<number>(0);

  useEffect(() => {
    if (selected_id) {
      appDispatch(ActionsStore.fetchAvailableActions(selected_id));
    }
  }, [selected_id, numAction]);

  useEffect(() => {
    const updatedParameters: Record<string, IActionParameterDTO[]> = {};
    actions.forEach((action) => {
      updatedParameters[action.name] = action.parameters.map((p) => ({ ...p }));
    });
    setActionParameters(updatedParameters);
    setExpandedAction(null);
  }, [actions]);

  const executeAction = async (action: IActionDescrDTO) => {
    if (!selected_id) return;

    const params = actionParameters[action.name] ?? [];
    const payload: IActionExeDTO = {
      object_id: selected_id,
      name: action.name!,
      parameters: params,
    };

    await appDispatch(ActionsStore.executeAction(payload));

    setNumAction(numAction + 1);
    setExpandedAction(null);
  };

  const handleAccordionChange = (action: IActionDescrDTO) => {
    setExpandedAction((prev) => (prev === action.name ? null : action.name));
  };

  const handleParameterChange = (actionName: string, index: number, value: string) => {
    setActionParameters((prev) => ({
      ...prev,
      [actionName]: prev[actionName].map((p, i) =>
        i === index ? { ...p, cur_val: value } : p
      ),
    }));
  };

  return (
    <Box sx={{ padding: 2 }}>
      <List dense>
        {actions.map((action) => (
          <ListItem key={action.name} sx={{ padding: 0 }}>
            <Accordion
              expanded={expandedAction === action.name}
              onChange={() => handleAccordionChange(action)}
              sx={{ width: '100%' }}
            >
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                {action.name}
              </AccordionSummary>
              <AccordionDetails sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                {actionParameters[action.name]?.map((param, index) => (
                  <TextField
                    key={param.name}
                    label={param.name}
                    value={param.cur_val ?? ''}
                    onChange={(e) =>
                      handleParameterChange(action.name, index, e.target.value)
                    }
                    fullWidth
                    margin="dense"
                  />
                ))}
                <Button
                  onClick={() => executeAction(action)}
                  color="primary"
                  variant="contained"
                  fullWidth
                >
                  Execute
                </Button>
              </AccordionDetails>
            </Accordion>
          </ListItem>
        ))}
      </List>
    </Box>
  );
}
