﻿/* eslint-disable react-hooks/exhaustive-deps */
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
import { IActionDescrDTO, IActionParameterDTO, IActionExeDTO, PointType, IPointCoord } from '../store/Marker';
import { useAppDispatch } from '../store/configureStore';
import { useSelector } from 'react-redux';
import { ApplicationState } from '../store';
import * as ActionsStore from '../store/ActionsStates';
import CoordInput from '../prop_controls/CoordInput';

export function ActionsControl() {
  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const actions = useSelector((state: ApplicationState) => state?.actionsStates?.actions);

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

    if (actions) {
      actions.forEach((action) => {
        updatedParameters[action.name] = action.parameters.map((p) => ({ ...p }));
      });
    }
    
    setActionParameters(updatedParameters);
    setExpandedAction(null);
  }, [actions]);

  const executeAction = async (action: IActionDescrDTO) => {
    // Если нет выбранного объекта, выходим из функции
    if (!selected_id) return;

    // Получаем параметры действия или создаем пустой массив, если их нет
    const params = actionParameters[action.name] ?? [];

    // Создаем полезную нагрузку для выполнения действия
    const payload: IActionExeDTO = {
      object_id: selected_id,  // ID объекта для выполнения действия
      name: action.name!,  // Название действия
      parameters: params.map((param) => {
        // Для каждого параметра проверяем его тип
        if (param.type === 'coordinates' && Array.isArray(param.cur_val)) {
          // Если это координаты, преобразуем их в строку "lat,lon"
          const [lat, lon] = param.cur_val;
          return {
            ...param,
            cur_val: `${lat},${lon}`,
          };
        }
        // Если это не координаты, оставляем параметр без изменений
        return { ...param };
      }),
    };

    // Выполняем действие с подготовленной полезной нагрузкой
    await appDispatch(ActionsStore.executeAction(payload));

    // Перезапрашиваем действия после выполнения
    setNumAction(numAction + 1);

    // Закрываем аккордеон
    setExpandedAction(null);
  };


  const handleAccordionChange = (action: IActionDescrDTO) => {
    setExpandedAction((prev) => (prev === action.name ? null : action.name));
  };

  const handleParameterChange = (
    actionName: string,
    index: number,
    value: string | number | { lat: number | null; lon: number | null }
  ) => {
    // Получаем старое состояние для actionName
    const previousActionParams = actionParameters[actionName];

    // Создаем новый массив для обновленных значений
    const updatedParams = previousActionParams?.map((param, i) => {
      if (i === index) {
        // Если это координаты, преобразуем их в IPointCoord
        if (value && typeof value === 'object' && 'lat' in value && 'lon' in value) {
          // Если координаты валидные (lat и lon не null), создаем объект IPointCoord
          const newCoordValue: IPointCoord | null = value.lat && value.lon
            ? { type: PointType, coord: [value.lat, value.lon] }
            : null;

          // Возвращаем новый параметр с обновленным значением cur_val
          return { ...param, cur_val: newCoordValue };
        }

        // Если это не координаты, просто присваиваем значение cur_val
        return { ...param, cur_val: value };
      }
      return param;
    }) ?? []; // Если previousActionParams == null, возвращаем пустой массив

    // Обновляем только нужное значение в состоянии
    setActionParameters((prev) => ({
      ...prev,                          // Копируем старое состояние
      [actionName]: updatedParams,      // Обновляем только те параметры, которые изменились
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
                {actionParameters[action.name]?.map((param, paramIndex) => {
                  if (param.type === 'coordinates') {
                    const [lat, lon] = param.cur_val.coord; // Извлекаем lat и lon из массива LatLngPair

                    return (
                      <CoordInput
                        key={param.name}
                        index={0}
                        lat={lat ?? 0} // Используем 0 как значение по умолчанию, если lat == null
                        lng={lon ?? 0} // Используем 0 как значение по умолчанию, если lon == null
                        onCoordChange={(index, lat, lng) =>
                          handleParameterChange(action.name, paramIndex, { lat, lon: lng })
                        }
                      />
                    );
                  }
                  return (
                    <TextField
                      key={param.name}
                      label={param.name}
                      type={param.type === 'int' || param.type === 'double' ? 'number' : 'text'}
                      value={param.cur_val ?? ''}
                      onChange={(e) =>
                        handleParameterChange(action.name, paramIndex, e.target.value)
                      }
                      fullWidth
                      margin="dense"
                    />
                  );
                })}
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
