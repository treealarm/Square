import * as React from 'react';
import { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { PolygonTool, PolylineTool, CircleTool } from '../store/EditStates';
import { ICircle, IFigures, IPolygon, IPolyline, Marker } from '../store/Marker';
import { CircleMaker } from "./CircleMaker";
import { PolygonMaker } from "./PolygonMaker";
import { PolylineMaker } from "./PolylineMaker";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as EditStore from '../store/EditStates';

export function EditableFigure(props: any) {

  const dispatch = useDispatch();
  const selectedEditMode = useSelector((state) => state.editState);
  const [obj2Edit, setObj2Edit] = React.useState<Marker>(null);
  const guiStates = useSelector((state) => state?.guiStates);
  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const markers = useSelector((state) => state?.markersStates?.markers);

  const polygonChanged = useCallback(
    (polygon: IPolygon, e) => {
      var figures: IFigures = {

      };
      setObj2Edit(null);
      figures.polygons = [polygon];
      dispatch(MarkersStore.actionCreators.sendMarker(figures));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  const polylineChanged = useCallback(
    (figure: IPolyline, e) => {
      var figures: IFigures = {

      };
      setObj2Edit(null);
      figures.polylines = [figure];
      dispatch(MarkersStore.actionCreators.sendMarker(figures));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  const circleChanged = useCallback(
    (figure: ICircle, e) => {
      var figures: IFigures = {

      };
      setObj2Edit(null);
      figures.circles = [figure];
      dispatch(MarkersStore.actionCreators.sendMarker(figures));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  useEffect(() => {
    let map_center = guiStates.map_option?.map_center;
    map_center = map_center ? map_center : [51.5359, -0.09];
    //parentMap.setView(map_center);
  }, [guiStates.map_option?.map_center]);

  useEffect(() => {
    if (selectedEditMode.figure != EditStore.NothingTool) {

      if (!selectedEditMode.edit_mode) {
        setObj2Edit(null);
        return;
      }

      if (selected_id != null && selectedEditMode.edit_mode) {
        let circle = markers?.circles.find(f => f.id == selected_id);

        if (circle != null) {
          setObj2Edit(circle);
          return;
        }

        let polygon = markers?.polygons.find(f => f.id == selected_id);

        if (polygon != null) {
          setObj2Edit(polygon);
          return;
        }

        let polyline = markers?.polylines.find(f => f.id == selected_id);

        if (polyline != null) {
          setObj2Edit(polyline);
          return;
        }
      }
    }

    if (selectedEditMode.figure == EditStore.NothingTool && selected_id != null) {
      setObj2Edit(null);
    }
  }, [selected_id, selectedEditMode]);

  return (
    <React.Fragment>
      {
        selectedEditMode.figure == PolygonTool ?
          <PolygonMaker polygonChanged={polygonChanged} obj2Edit={obj2Edit} /> : <div />
      }
      {
        selectedEditMode.figure == PolylineTool ?
          <PolylineMaker figureChanged={polylineChanged} obj2Edit={obj2Edit} /> : <div />
      }
      {
        selectedEditMode.figure == CircleTool ?
          <CircleMaker figureChanged={circleChanged} obj2Edit={obj2Edit} /> : <div />
      }

    </React.Fragment >
  );
}