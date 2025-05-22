/* eslint-disable react-hooks/exhaustive-deps */
import React, { useEffect, useState, useRef } from 'react';
import { useSelector } from 'react-redux';
import { Box, LinearProgress, Typography, Card, CardContent, Button } from '@mui/material';
import { ApplicationState } from '../store';
import * as IntegroStore from '../store/IntegroStates';
import { useAppDispatch } from '../store/configureStore';

interface ActionExecutionListProps {
  objectId: string | null;
  maxProgress?: number;
}

const ActionExecutionList: React.FC<ActionExecutionListProps> = ({ objectId, maxProgress = 99 }) => {
  const appDispatch = useAppDispatch();
  const actions = useSelector((state: ApplicationState) => state.integroStates.actionsByObject);
  const [intervalMs, setIntervalMs] = useState(1000);
  const intervalRef = useRef<number | null>(null);

  useEffect(() => {
    if (!objectId) return;

    const fetchData = () => {
      appDispatch(IntegroStore.fetchActionsByObjectId({ object_id: objectId, max_progress: maxProgress }));
    };

    fetchData(); // initial fetch

    intervalRef.current = window.setInterval(fetchData, intervalMs);

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [objectId, intervalMs, maxProgress]);

  // Обновляем интервал, если прогресс >= 100
  useEffect(() => {
    if (!actions || actions.length === 0) return;

    const allDone = actions.every(
      (action) => (action.result?.progress ?? 0) >= 100
    );

    const newInterval = allDone ? 5000 : 1000;
    if (newInterval !== intervalMs) {
      setIntervalMs(newInterval);
    }
  }, [actions]);

  if (!objectId || !actions || actions.length === 0) {
    return null;
  }

  return (
    <Box>
      {actions.map((action) => {
        const progress = action.result?.progress ?? 0;
        const executionId = action.result?.action_execution_id;

        const handleCancel = () => {
          if (executionId) {
            appDispatch(IntegroStore.cancelAction(executionId));
          }
        };

        return (
          <Card key={action.name + action.object_id} sx={{ mb: 2 }}>
            <CardContent>
              <Typography variant="h6">{action.name || 'no name'}</Typography>
              <LinearProgress variant="determinate" value={Math.min(progress, 100)} sx={{ mt: 1 }} />
              <Typography variant="body2" sx={{ mt: 1 }}>
                Progress: {progress}%
              </Typography>
              {progress < 100 && executionId && (
                <Button variant="outlined" color="error" size="small" onClick={handleCancel} sx={{ mt: 1 }}>
                  Cancel
                </Button>
              )}
            </CardContent>
          </Card>
        );
      })}

    </Box>
  );
};

export default ActionExecutionList;
