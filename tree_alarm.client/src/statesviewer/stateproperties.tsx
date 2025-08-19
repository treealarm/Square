import { useSelector } from "react-redux";
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemText,
  Divider
} from "@mui/material";

import { ApplicationState } from "../store";
import { ObjectStateDTO, ObjectStateDescriptionDTO } from "../store/Marker";

export function StateProperties() {
  const selectedState: ObjectStateDTO | null = useSelector(
    (state: ApplicationState) => state?.markersVisualStates?.selected_state ?? null
  );

  const stateDescriptions: ObjectStateDescriptionDTO[] = useSelector(
    (state: ApplicationState) => state?.markersVisualStates?.visualStates?.states_descr ?? []
  );

  if (!selectedState) {
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="h6">Not selected</Typography>
      </Box>
    );
  }

  // для текущего объекта ищем описания его states[]
  const descriptions: ObjectStateDescriptionDTO[] = stateDescriptions.filter(d =>
    selectedState.states.includes(d.state)
  );


  return (
    <Box sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
       Properties
      </Typography>

      <Typography variant="subtitle1">ID:</Typography>
      <Typography variant="body1" gutterBottom>
        {selectedState.id}
      </Typography>

      <Divider sx={{ my: 1 }} />

      <Typography variant="subtitle1">States:</Typography>
      <List dense>
        {descriptions.map(desc => (
          <ListItem key={desc.id}>
            <ListItemText
              primary={
                <span style={{ color: desc.state_color }}>
                  {desc.state_descr}
                </span>
              }
              secondary={`Code: ${desc.state}, Alarm: ${desc.alarm}`}
            />
          </ListItem>
        ))}
      </List>
    </Box>
  );
}
