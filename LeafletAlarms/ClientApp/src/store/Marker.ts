export interface GeoPart {
  coordinates: number[];
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