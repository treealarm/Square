import {ToggleButton, Tooltip} from "@mui/material";

import * as PanelsStore from '../store/PanelsStates';
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { DeepCopy, IPanelsStatesDTO } from "../store/Marker";
import { useAppDispatch } from "../store/configureStore";

import { PanelIcon } from "./PanelIcon";

export const TogglePanelButton = (props: { panel: IPanelsStatesDTO }) => {
  const appDispatch = useAppDispatch();

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var thisPanel = panels.find(p => p.panelId == props.panel.panelId);

  const handleSelectRight = (panelId: string, text: string) => {

    var exist = panels.find(e => e.panelId == panelId && e.panelType == e.panelType);

    if (exist) {
      //var removed = panels.filter(e => e.panelId != panelId);
      //appDispatch(PanelsStore.set_panels(removed));
    }
    else {
      var newPanels = DeepCopy(panels);

      newPanels = newPanels.filter(e => e.panelType != props.panel.panelType);
      newPanels.push(
        {
          panelId: panelId,
          panelValue: text,
          panelType: props.panel.panelType
        });

      appDispatch(PanelsStore.set_panels(newPanels));
    }
  };

  return (
    <Tooltip title={props.panel.panelValue}>
      <ToggleButton
        value="check"
        aria-label="properties"
        selected={thisPanel != null}
        size="small"
        onChange={() =>
          handleSelectRight(props.panel.panelId,
            props.panel.panelValue)
        }
      >
        <PanelIcon panelId={props.panel.panelId} />
      </ToggleButton>
    </Tooltip>
  );
}