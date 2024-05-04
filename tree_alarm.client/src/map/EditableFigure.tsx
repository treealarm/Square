import * as React from 'react';
import { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { getExtraProp, ICircle, IFigures, IPolygon, IPolyline, LineStringType, PointType, PolygonType } from '../store/Marker';
import { CircleMaker } from "./CircleMaker";
import { PolygonMaker } from "./PolygonMaker";
import { PolylineMaker } from "./PolylineMaker";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';

export function EditableFigure() {

  const dispatch = useDispatch();

  const obj2Edit = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const selectedEditMode = useSelector((state: ApplicationState) => state.editState);

  const polygonChanged = useCallback(
    (polygon: IPolygon, e: any) => {
      var figures: IFigures = {

      };
      figures.figs = [polygon];
      dispatch<any>(MarkersStore.actionCreators.updateFigures(figures));
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  const polylineChanged = useCallback(
    (figure: IPolyline, e:any) => {
      var figures: IFigures = {

      };
      figures.figs = [figure];
      dispatch<any>(MarkersStore.actionCreators.updateFigures(figures));
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  const circleChanged = useCallback(
    (figure: ICircle, e: any) => {
      var figures: IFigures = {

      };
      figures.figs = [figure];
      dispatch<any>(MarkersStore.actionCreators.updateFigures(figures));
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
    }, [])


  if (obj2Edit == null) {
    return null;
  }  

  if (!selectedEditMode.edit_mode) {
    return null;
  }

  var geometry = JSON.parse(getExtraProp(obj2Edit, "geometry"));

  return (
    <React.Fragment>
      {
        geometry.type == PolygonType ?
          <PolygonMaker figureChanged={polygonChanged} obj2Edit={obj2Edit} /> : <div />
      }
      {
        geometry.type == LineStringType ?
          <PolylineMaker figureChanged={polylineChanged} obj2Edit={obj2Edit} /> : <div />
      }
      {
        geometry.type == PointType ?
          <CircleMaker figureChanged={circleChanged} obj2Edit={obj2Edit} /> : <div />
      }

    </React.Fragment >
  );
}