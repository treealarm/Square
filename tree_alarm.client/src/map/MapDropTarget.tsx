/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect } from 'react';
import { useMap } from 'react-leaflet';
import { useAppDispatch, theStore } from '../store/configureStore';
import * as MarkersStore from '../store/MarkersStates';
import * as ObjPropsStore from '../store/ObjPropsStates';
import * as GuiStore from '../store/GUIStates';
import { ICircle, IObjProps, PointType, DeepCopy, getExtraProp, setExtraProp } from '../store/Marker';
import { fetchIntegroInfoByIds } from '../tree/integroInfo';
import { getDefaultIcon } from '../tree/defaultIcons';
import { TREE_MARKER_DRAG_TYPE, OBJECT_REPLICA_DRAG_TYPE } from '../tree/dragTypes';

// Lets a tree item be dropped directly onto the map to place its geo in one motion,
// instead of "Add Geo" -> toggle edit mode -> click map -> Save.
export function MapDropTarget() {
  const map = useMap();
  const appDispatch = useAppDispatch();

  useEffect(() => {
    const container = map.getContainer();

    const onDragOver = (e: DragEvent) => {
      if (e.dataTransfer?.types.includes(TREE_MARKER_DRAG_TYPE) ||
          e.dataTransfer?.types.includes(OBJECT_REPLICA_DRAG_TYPE)) {
        e.preventDefault();
      }
    };

    // Falls back to a producer's default icon only when the figure doesn't already carry one
    // (the replica branch below pre-fills __image from a snapshot of the owner's extra_props,
    // so this is a no-op there unless the owner itself had no icon).
    const applyDefaultIconFallback = async (figure: ICircle, sourceId: string) => {
      const [integroInfo] = await fetchIntegroInfoByIds([sourceId]).catch(() => []);
      const defaultIcon = getDefaultIcon(integroInfo?.i_name, integroInfo?.i_type);
      if (defaultIcon && !getExtraProp(figure, '__image')) {
        setExtraProp(figure, '__image', defaultIcon.image, 'String');
        setExtraProp(figure, '__image_rotate', '0', 'Int');
      }
    };

    const onDrop = async (e: DragEvent) => {
      const rect = container.getBoundingClientRect();
      const latlng = map.containerPointToLatLng([e.clientX - rect.left, e.clientY - rect.top]);

      // Dropped the Properties panel's replica handle — create a brand-new object owned by the
      // dragged one, instead of moving the dragged object's own geometry.
      const ownerId = e.dataTransfer?.getData(OBJECT_REPLICA_DRAG_TYPE);
      if (ownerId) {
        e.preventDefault();

        const owner = await ObjPropsStore.requestObjPropsById(ownerId);
        if (!owner) return;

        const newObjProps: IObjProps = {
          id: null,
          name: owner.name,
          parent_id: owner.parent_id,
          owner_id: ownerId,
          extra_props: DeepCopy(owner.extra_props) ?? [],
        };
        const created = await appDispatch(ObjPropsStore.updateObjProps(newObjProps)).unwrap();

        const figure: ICircle = {
          ...created,
          geometry: { type: PointType, coord: [latlng.lat, latlng.lng] },
          radius: 100,
        };
        await applyDefaultIconFallback(figure, ownerId);

        appDispatch(MarkersStore.updateMarkers({ figs: [figure] }));
        // So the new node shows up under the owner's parent right away.
        appDispatch(GuiStore.requestTreeUpdate());
        appDispatch(GuiStore.selectTreeItem(created.id));
        return;
      }

      const id = e.dataTransfer?.getData(TREE_MARKER_DRAG_TYPE);
      if (!id) return;
      e.preventDefault();

      const [base, existing] = await Promise.all([
        ObjPropsStore.requestObjPropsById(id),
        MarkersStore.requestMarkersByIds([id]),
      ]);
      if (!base) return;

      const existingFig = existing.figs?.[0];

      // A Point is just a pin — dragging again moves it. Polygon/LineString carry real
      // shape data a drag shouldn't silently destroy, so leave those alone.
      if (existingFig?.geometry?.type && existingFig.geometry.type !== PointType) return;

      const figure: ICircle = {
        ...base,
        geometry: { type: PointType, coord: [latlng.lat, latlng.lng] },
        radius: existingFig?.radius ?? 100,
      };

      await applyDefaultIconFallback(figure, id);

      appDispatch(MarkersStore.updateMarkers({ figs: [figure] }));

      // If this object is the one currently open in the Properties panel, its editor copy
      // (markersStates.selected_marker) is a separate piece of state that wouldn't otherwise
      // see this new geometry — left stale, clicking "Save" there would overwrite the drop
      // with the old position.
      if (theStore.getState().guiStates?.selected_id === id) {
        appDispatch(MarkersStore.selectMarkerLocally(figure));
      }
    };

    container.addEventListener('dragover', onDragOver);
    container.addEventListener('drop', onDrop);
    return () => {
      container.removeEventListener('dragover', onDragOver);
      container.removeEventListener('drop', onDrop);
    };
  }, [map, appDispatch]);

  return null;
}
