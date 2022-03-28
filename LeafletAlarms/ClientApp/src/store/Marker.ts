export interface GeoPart {
  lng: number;
  lat: number;
}

export interface Marker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  geometry: GeoPart;
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