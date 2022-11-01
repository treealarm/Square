

export type LatLngPair = [number, number];

export interface Marker {
  id: string;
  parent_id?: string | null;
  name: string;
}

export const PointType = 'Point';
export const PolygonType = 'Polygon';
export const LineStringType = 'LineString';

export interface IPointCoord extends IGeometryDTO {
  type: 'Point';
  coord: LatLngPair;
}

export interface IPolygonCoord extends IGeometryDTO {
  type: 'Polygon';
  coord: LatLngPair[];
}

export interface IPolylineCoord extends IGeometryDTO {
  type: 'LineString';
  coord: LatLngPair[];
}

export interface ICommonFig extends IObjProps {
  geometry: any;
  radius?: number;
}

export interface ICircle extends ICommonFig {
  geometry: IPointCoord;  
  radius: number;
}

export interface IPolygon extends ICommonFig {
  geometry: IPolygonCoord;  
}

export interface IPolyline extends ICommonFig {
  geometry: IPolylineCoord;  
}

export interface ObjExtraPropertyDTO {
  visual_type?: string;
  str_val: string;
  prop_name: string;
}

export interface IObjProps extends Marker {
  extra_props?: ObjExtraPropertyDTO[];
  zoom_min?: number;
  zoom_max?: number;
}

export function setExtraProp(
  obj: IObjProps,
  propName: string,
  propValue: string,
  visual_type: string
) {
  if (obj.extra_props == null) {
    obj.extra_props = [{
      prop_name: propName,
      str_val: propValue,
      visual_type: visual_type
    }];
    return;
  }
  var newElem: ObjExtraPropertyDTO = {
    prop_name: propName,
    str_val: propValue,
    visual_type: visual_type
  };

  var g = obj?.extra_props.find(p => p.prop_name == propName);
  if (g != null) {
    Object.assign(g, newElem);
    return;
  }
  obj.extra_props = [...obj.extra_props,...[newElem]];
}

export function getExtraProp(
  obj: IObjProps,
  propName: string
): string {
  if (obj?.extra_props == null) {
    return null;
  }
  var g = obj?.extra_props?.find(p => p.prop_name == propName);
  return g?.str_val;
}

export interface IFigures {
  figs?: ICommonFig[];
}

export interface TreeMarker {
  id: string;
  parent_id?: string | null;
  name: string;
  has_children?: boolean;
}

export interface GetByParentDTO {
  parent_id?: string | null;
  parents?: TreeMarker[];
  children?: TreeMarker[];
  start_id?: string;
  end_id?: string;
}

export interface GetBySearchDTO {
  list?: TreeMarker[];
  search_id: string;
}

export interface BoundBox {
  wn: number[];
  es: number[];
  zoom: number;
  ids?: string[];
  count?: number;
}

export interface KeyValueDTO {
  str_val: string
  prop_name: string
}

export interface ObjPropsSearchDTO {
  props?: KeyValueDTO[];
}

export interface SearchFilterGUI {
  time_start: Date;
  time_end: Date;  
  property_filter?: ObjPropsSearchDTO;
  search_id: string;
  sort?: number;
}

export interface SearchFilterDTO{
  time_start?: Date;
  time_end?: Date;
  property_filter?: ObjPropsSearchDTO;
  search_id: string;
  start_id?: string;
  forward: boolean;
  count: number;
}


export interface BoxTrackDTO extends BoundBox
{
  time_start?: Date;
  time_end?: Date;
  property_filter?: ObjPropsSearchDTO;
  sort?: number;
}

export interface ViewOption {
  map_center: LatLngPair;
  zoom?: number;
}

export interface ObjectStateDTO {
  id: string;
  states: string[];
}

export interface ObjectStateDescriptionDTO {
  id: string;
  alarm: boolean;
  state: string;
  state_descr: string;
  state_color: string;
  external_type: string
}

export interface AlarmObject {
  id: string;
  alarm: boolean;
  children_alarms: number;
}

export interface MarkerVisualStateDTO {
  states: ObjectStateDTO[];
  states_descr: ObjectStateDescriptionDTO[];  
}

export interface IGeometryDTO {
  coord: any[];
  type: string;
}

export interface IGeoObjectDTO {
  id: string;
  radius: number;
  zoom_level: string;
  
  location: IGeometryDTO;
}

export interface IRoutLineDTO {
  id: string;
  id_start: string;
  id_end: string;
  figure: IGeoObjectDTO;
  ts_start: Date;
  ts_end: Date;
}

export interface ITrackPointDTO {
  id: string;
  timestamp: Date;
  figure: IGeoObjectDTO;
}
