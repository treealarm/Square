/* eslint-disable no-unused-vars */
import { useSelector } from "react-redux";
import { Box, Typography, List, ListItem, ListItemText, Divider } from "@mui/material";
import { useMemo } from "react";
import { ApplicationState } from "../store";
import { IObjProps, MarkerVisualStateDTO, ObjectStateDescriptionDTO, VisualTypes } from "../store/Marker";
import { ControlSelector } from "../prop_controls/control_selector";
import { SnapshotSimpleViewer } from "../prop_controls/SnapshotSimpleViewer";

export function StateProperties()
{
  const objProps: IObjProps | null = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps ?? null);

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

  var item = objProps?.extra_props?.find(p => p.visual_type == VisualTypes.SnapShot);

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
        {
          item ? <ListItem>
            <ControlSelector
              prop_name={item?.prop_name}
              str_val={item?.str_val}
              visual_type={item?.visual_type ?? null}
              handleChangeProp={() => { }}
              object_id={objProps?.id ?? null} />
          </ListItem> : <Divider sx={{ my: 1 }} />
        }
      <ListItem>
          <SnapshotSimpleViewer imageSrc={item?.str_val ?? ""} />
      </ListItem>
      </List>
    </Box>
  );
}
