/* eslint-disable no-unused-vars */
import { useSelector } from "react-redux";
import { Box, Typography, List, ListItem, ListItemText, Divider } from "@mui/material";
import { useMemo } from "react";
import { ApplicationState } from "../store";
import { MarkerVisualStateDTO, ObjectStateDescriptionDTO } from "../store/Marker";

export function StateProperties()
{
  const selected_state = useSelector(
    (state: ApplicationState) =>
      state?.markersVisualStates?.visualStates?.states.find(
        s => s.id === state?.markersVisualStates?.selected_state_id
      ) ?? null
  );

  const stateDescriptions = useSelector(
    (state: ApplicationState) => state?.markersVisualStates?.visualStates?.states_descr ?? []
  );

  let descriptions: ObjectStateDescriptionDTO[] = [];

  if (selected_state) {
    descriptions = stateDescriptions.filter(d => selected_state.states.includes(d.state));
  }



  if (!selected_state) {
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="h6">Not selected</Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Properties
      </Typography>

      <Typography variant="subtitle1">ID:</Typography>
      <Typography variant="body1" gutterBottom>
        {selected_state.id}
      </Typography>

      <Divider sx={{ my: 1 }} />

      <Typography variant="subtitle1">States:</Typography>
      <List dense>
        {descriptions.map(desc => (
          <ListItem key={desc.state + desc.id}>
            <ListItemText
              primary={<span style={{ color: desc.state_color }}>{desc.state_descr}</span>}
              secondary={`Code: ${desc.state}, Alarm: ${desc.alarm}`}
            />
          </ListItem>
        ))}
      </List>
    </Box>
  );
}
