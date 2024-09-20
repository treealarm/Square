import { IPointCoord, IPolygonCoord, IPolylineCoord } from "../store/Marker";

export interface IControlSelector {
  prop_name: string;
  str_val: any;
  visual_type: string | null;
  // eslint-disable-next-line no-unused-vars
  handleChangeProp: (e: any) => void;
}

export interface IControlGeoProps {
  prop_name: string;
  val: IPointCoord | IPolygonCoord | IPolylineCoord;
  // eslint-disable-next-line no-unused-vars
  handleChangeProp: (e: any) => void;
}