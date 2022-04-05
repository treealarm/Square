
export type LatLngPair = [number, number];

export interface Marker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  type: string;
}

export interface ICircle extends Marker {
  geometry: LatLngPair;
}

export interface IPolygon extends Marker {
  geometry: LatLngPair[];
}


export interface IFigures {
  circles?: ICircle[];
  polygons?: IPolygon[];
}


export interface TreeMarker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  has_children?: boolean;
}

export interface BoundBox {
  wn: number[];
  es: number[];
  zoom: number;
}
