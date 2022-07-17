import * as React from 'react';
import { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { ICircle, IFigures, IPolygon, IPolyline, LineStringType, PointType, PolygonType } from '../store/Marker';
import { CircleMaker } from "./CircleMaker";
import { PolygonMaker } from "./PolygonMaker";
import { PolylineMaker } from "./PolylineMaker";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';

export function EditableFigure() {

  const dispatch = useDispatch();

  const obj2Edit = useSelector((state) => state?.objPropsStates?.objProps);
  const selectedEditMode = useSelector((state) => state.editState);

  const polygonChanged = useCallback(
    (polygon: IPolygon, e) => {
      var figures: IFigures = {

      };
      figures.polygons = [polygon];
      dispatch(MarkersStore.actionCreators.addTracks(figures));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  const polylineChanged = useCallback(
    (figure: IPolyline, e) => {
      var figures: IFigures = {

      };
      figures.polylines = [figure];
      dispatch(MarkersStore.actionCreators.addTracks(figures));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  const circleChanged = useCallback(
    (figure: ICircle, e) => {
      var figures: IFigures = {

      };
      figures.circles = [figure];
      dispatch(MarkersStore.actionCreators.addTracks(figures));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])


  if (obj2Edit == null) {
    return null;
  }  

  if (!selectedEditMode.edit_mode) {
    return null;
  }

  return (
    <React.Fragment>
      {
        obj2Edit.type == PolygonType ?
          <PolygonMaker figureChanged={polygonChanged} obj2Edit={obj2Edit} /> : <div />
      }
      {
        obj2Edit.type == LineStringType ?
          <PolylineMaker figureChanged={polylineChanged} obj2Edit={obj2Edit} /> : <div />
      }
      {
        obj2Edit.type == PointType ?
          <CircleMaker figureChanged={circleChanged} obj2Edit={obj2Edit} /> : <div />
      }

    </React.Fragment >
  );
}