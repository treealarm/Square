import * as React from 'react';

import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';

import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import DiagramElement from './DiagramElement';

interface IDiagramContent {
  parent_id: string;
  zoom: number;
}

export default function DiagramContent(props: IDiagramContent) {

  const diagrams = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  if (diagrams == null) {
    return null;
  }

  var content = diagrams.content.filter(e => e.parent_id == props.parent_id);
  var zoom = props.zoom;

  return (
    <React.Fragment>

      {
        content.map((dgr, index) =>
          <React.Fragment>
            <DiagramElement diagram={dgr} zoom={zoom} />
          </React.Fragment>
        )}

    </React.Fragment>
  );
}