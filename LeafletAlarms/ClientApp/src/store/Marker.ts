﻿
export type LatLngPair = [number, number];

export interface Marker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  type: string;
}

export const PointType = 'Point';
export const PolygonType = 'Polygon';
export const LineStringType = 'LineString';

export interface ICoord {
  coord: LatLngPair;
}

export interface IArrayCoord {
  coord: LatLngPair[];
}

export interface ICircle extends Marker {
  geometry: ICoord;
  type: typeof PointType;
  radius: number;
}

export interface IPolygon extends Marker {
  geometry: IArrayCoord;
  type: typeof PolygonType;
}

export interface IPolyline extends Marker {
  geometry: IArrayCoord;
  type: typeof LineStringType;
}

export interface ObjExtraPropertyDTO {
  visual_type: string;
  str_val: string;
  prop_name: string;
}

export interface IObjProps extends Marker {
  extra_props: ObjExtraPropertyDTO[];
  geometry: string;
}

export interface IFigures {
  circles?: ICircle[];
  polygons?: IPolygon[];
  polylines?: IPolyline[];
}

export interface TreeMarker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  has_children?: boolean;
}

export interface GetByParentDTO {
  parent_id?: string | null;
  parents?: TreeMarker[];
  children?: TreeMarker[];
}

export interface BoundBox {
  wn: number[];
  es: number[];
  zoom: number;
}

export interface ViewOption {
  map_center: LatLngPair;
}

export interface MarkerVisualState {
  id: string;
  color: string;
}

export interface IGeometryDTO {
  coord: any[];
}

export interface IGeoObjectDTO {
  id: string;
  radius: number;
  zoom_level: string;
  location: IGeometryDTO;
}

export interface ITrackPointDTO {
  id: string;
  timestamp: Date;
  figure: IGeoObjectDTO;
}
