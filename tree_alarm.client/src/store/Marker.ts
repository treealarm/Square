export function DeepCopy<T>(a: T): T {
  if (a == null) {
    return null;
  }
  return JSON.parse(JSON.stringify(a));
}

export function uuidv4() {
  return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
    (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
  );
}

export type LatLngPair = [number, number];

export interface Marker {
  id: string|null;
  parent_id?: string | null;
  name: string;
}

export const PointType = 'Point' as const;
export const PolygonType = 'Polygon' as const;
export const LineStringType = 'LineString' as const;

export type GeometryType = typeof PointType | typeof PolygonType | typeof LineStringType;

export interface IGeometryDTO<T extends GeometryType = any, C = any> {
  coord: C;
  type: T;
}


export interface IPointCoord extends IGeometryDTO<typeof PointType, LatLngPair | null> {
}

export interface IPolygonCoord extends IGeometryDTO<typeof PolygonType, LatLngPair[]> {
}

export interface IPolylineCoord extends IGeometryDTO<typeof LineStringType, LatLngPair[]> {
}

export function getGeometryType(geometry: any): IGeometryDTO<any, any> | null {
  if (!geometry || typeof geometry !== 'object') {
    return null; // Явно возвращаем null для недопустимых значений
  }

  switch (geometry.type) {
    case PointType:
      return geometry as IPointCoord;
    case PolygonType:
      return geometry as IPolygonCoord;
    case LineStringType:
      return geometry as IPolylineCoord;
    default:
      return null; // Явно возвращаем null для неизвестных типов
  }
}



export interface ICommonFig extends IObjProps {
  geometry: IPointCoord | IPolygonCoord | IPolylineCoord;
  radius?: number;
  zoom_level?: string;
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

export interface IRoutDTO {
  InstanceName: string;
  Profile: string;
  Coordinates: LatLngPair[];
}
export interface ObjExtraPropertyDTO {
  visual_type?: string;
  str_val: string;
  prop_name: string;
}

export interface IObjProps extends Marker {
  extra_props?: ObjExtraPropertyDTO[]|null;
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
  propName: string,
  defVal:string|null = null
): string {

  if (obj?.extra_props == null) {
    return defVal;
  }
  var g = obj?.extra_props?.find(p => p.prop_name == propName);
  if (g == null) {
    return defVal;
  }
  return g?.str_val;
}


export interface IFigures {
  figs?: ICommonFig[];
  add_tracks?: boolean;
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

export interface GetTracksBySearchDTO {
  list?: ITrackPointDTO[];
  search_id: string;
}


export interface BoundBox {
  wn: number[];
  es: number[];
  zoom: number;
  ids?: string[];
  count?: number;
  property_filter?: ObjPropsSearchDTO;
}

export interface KeyValueDTO {
  str_val: string
  prop_name: string
}

export interface ObjPropsSearchDTO {
  props?: KeyValueDTO[];
}

export interface SearchFilterGUI {
  time_start: string;
  time_end: string;  
  property_filter?: ObjPropsSearchDTO;
  search_id: string;
  sort?: number;

  show_objects?: boolean;
  show_tracks?: boolean;
  show_routes?: boolean;

  applied?: boolean;
}

type Order = 'asc' | 'desc' | undefined;
export interface ISortPair {
  key: string;
  order: Order;
}
export interface SearchFilterDTO{
  time_start?: string;
  time_end?: string;
  property_filter?: ObjPropsSearchDTO;
  search_id: string;
  start_id?: string;
  forward: number;
  count: number;
  sort?: ISortPair[];
}


export interface BoxTrackDTO extends BoundBox
{
  time_start?: string;
  time_end?: string;  
  sort?: number;
}

export interface ViewOption {
  map_center?: LatLngPair|null;
  zoom?: number|null;
  find_current_pos?: boolean;
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
}

export interface AlarmObject {
  id: string;
  alarm: boolean;
}

export interface MarkerVisualStateDTO {
  states: ObjectStateDTO[];
  states_descr: ObjectStateDescriptionDTO[]; 
  alarmed_objects: AlarmObject[];
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
  ts_start: string;
  ts_end: string;
}

export interface ITrackPointDTO {
  id: string;
  timestamp: string;
  figure: IGeoObjectDTO;
  extra_props?: ObjExtraPropertyDTO[];
}

export interface IObjectRightValueDTO{
  role: string;
  value: number;
}

export interface IObjectRightsDTO {
  id?: string;
  rights?: IObjectRightValueDTO[];
}


export interface IRightValuesDTO {
  rightName: string;
  rightValue: number;
}

export enum EPanelType {
  Left,
  Right
}

export interface IPanelsStatesDTO {
  panelId: string;
  panelValue: string;
  panelType: EPanelType;
}

export class IPanelTypes{
  static tree: string = 'tree';
  static search_result: string = 'search_result';  

  static properties: string = 'properties';
  static search: string = 'search';
  static track_props: string = 'track_props';

  static rights: string = 'rights';

  static panels: IPanelsStatesDTO[] = [
    {
      panelId: IPanelTypes.tree,
      panelValue: 'Tree',
      panelType: EPanelType.Left
    },
    {
      panelId: IPanelTypes.search_result,
      panelValue: 'Search result',
      panelType: EPanelType.Left
    },


    {
      panelId: IPanelTypes.search,
      panelValue: 'Search',
      panelType: EPanelType.Right
    },
    {
      panelId: IPanelTypes.properties,
      panelValue: 'Properties',
      panelType: EPanelType.Right
    },
    {
      panelId: IPanelTypes.track_props,
      panelValue: 'Track properties',
      panelType: EPanelType.Right
    },
    {
      panelId: IPanelTypes.rights,
      panelValue: 'Rights',
      panelType: EPanelType.Right
    }
  ];
}

export interface IDiagramCoord {
  left: number;
  top: number;
  width: number;
  height: number;
}
export interface IDiagramDTO extends ICommonFig {
  geometry: IDiagramCoord;
  dgr_type: string;
  region_id: string;
  background_img: string;
}

export interface IDiagramTypeRegionDTO {
  id: string;
  geometry: IDiagramCoord;
}
export interface IDiagramTypeDTO {
  id?: string|null;
  name: string
  src: string;
  regions: IDiagramTypeRegionDTO[];
}

export interface IGetDiagramDTO {
  content: IDiagramDTO[];
  dgr_types: IDiagramTypeDTO[];
  parent: IDiagramDTO;
  parents?: TreeMarker[];
  depth: number;
}

export interface IGetDiagramTypesDTO {
  dgr_types: IDiagramTypeDTO[];
}

export interface IGetDiagramTypesByFilterDTO {
  filter: string;
  start_id: string;
  forward: boolean;
  count: number;
}

export enum LogLevel {
  Trace = 0,
  Debug = 1,
  Information = 2,
  Warning = 3,
  Error = 4,
  Critical = 5,
  None = 6,
}
export interface IEventMetaDTO {
  extra_props?: ObjExtraPropertyDTO[];
  not_indexed_props?: ObjExtraPropertyDTO[];
}
export interface IEventDTO {
  meta: IEventMetaDTO;
  timestamp: string;
  id: string;
  object_id: string;
  event_name: string;
  event_priority: number;
}

export interface IIntegrationDTO {
  id: string;
  parent_id?: string | null;
  name: string;
}
export interface IIntegrationExDTO extends IIntegrationDTO {
  has_children: boolean;
}
export interface IGetIntegrationsDTO {
  children: IIntegrationExDTO[] | null;
  parent_id?: string | null;
}

export interface IIntegrationLeafDTO {
  id?: string | null;
  integration_id: string;
}

export interface IGetIntegrationLeafsDTO {
  children: IIntegrationLeafDTO[] | null;
  integration_id?: string | null;
}

export interface ILevelDTO {
  id: string | null;
  zoom_level?: string | null;
  zoom_min?: number | null;
  zoom_max?: number | null;
}
