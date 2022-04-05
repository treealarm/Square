export interface GeoPart {
  type: string;
}

export interface CircleGeo extends GeoPart {
  lng: number;
  lat: number;
}

export interface Marker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  geometry: any;
}

export interface ICircle extends Marker {
  geometry: CircleGeo;
}

export interface IFigures {
  circles?: ICircle[];
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

export interface Area {
  circles: Marker[];
}