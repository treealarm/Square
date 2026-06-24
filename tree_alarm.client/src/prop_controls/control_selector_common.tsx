import { IPointCoord, IPolygonCoord, IPolylineCoord } from "../store/Marker";

export interface IControlSelector {
  prop_name: string;
  str_val: any;
  visual_type: string | null;
  object_id: string|null;
  // eslint-disable-next-line no-unused-vars
  handleChangeProp: (e: any) => void;
  // Set when the owning object is registered by an external producer (see ObjectProperties.tsx) —
  // that producer is the source of truth for this value, so this panel must not let it be edited.
  readOnly?: boolean;
}

export interface IControlGeoProps {
  prop_name: string;
  val: IPointCoord | IPolygonCoord | IPolylineCoord;
  // eslint-disable-next-line no-unused-vars
  handleChangeProp: (e: any) => void;
}