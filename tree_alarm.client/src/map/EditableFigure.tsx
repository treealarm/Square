import * as React from 'react';
import { useCallback } from 'react';
import { useSelector } from 'react-redux';
import { getExtraProp, ICircle, IFigures, IPolygon, IPolyline, LineStringType, PointType, PolygonType } from '../store/Marker';
import { CircleMaker } from "./CircleMaker";
import { PolygonMaker } from "./PolygonMaker";
import { PolylineMaker } from "./PolylineMaker";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { useAppDispatch } from '../store/configureStore';

export function EditableFigure() {

  const dispatch = useAppDispatch();

  const obj2Edit = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const selectedEditMode = useSelector((state: ApplicationState) => state.editState);

  const polygonChanged = useCallback(
    (polygon: IPolygon) => {
      var figures: IFigures = {

      };
      figures.figs = [polygon];
      dispatch(MarkersStore.updateMarkers(figures));
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
    }, [dispatch])

  const polylineChanged = useCallback(
    (figure: IPolyline) => {
      var figures: IFigures = {

      };
      figures.figs = [figure];
      dispatch(MarkersStore.updateMarkers(figures));
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
    }, [dispatch])

  const circleChanged = useCallback(
    (figure: ICircle) => {
      var figures: IFigures = {

      };
      figures.figs = [figure];
      dispatch(MarkersStore.updateMarkers(figures));
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
    }, [dispatch])


  if (obj2Edit == null) {
    return null;
  }  

  if (!selectedEditMode?.edit_mode) {
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