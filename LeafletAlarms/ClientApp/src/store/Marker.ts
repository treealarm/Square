
export interface Marker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  has_children?: boolean;
  points: number[];
}

export interface TreeMarker {
  id?: string | null;
  parent_id?: string | null;
  name: string;
  has_children?: boolean;
}