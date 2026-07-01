// Default visual representation (icon + orientation) for objects coming from known
// integrations that don't set one themselves. Keyed by (i_name, i_type) rather than
// hardcoded to "camera" so a second entry can be added later without restructuring.
export const CAMERA_ICON_PATH = 'images/videocamera.png';
export const CAMERA_DGR_TYPE_NAME = 'camera';

export interface IDefaultIcon {
  image: string;
}

interface IDefaultIconEntry {
  // eslint-disable-next-line no-unused-vars
  match: (i_name?: string | null, i_type?: string | null) => boolean;
  icon: IDefaultIcon;
}

const DEFAULT_ICONS: IDefaultIconEntry[] = [
  {
    match: (i_name, i_type) => i_name === 'vmscfg' && i_type === 'camera',
    icon: { image: CAMERA_ICON_PATH },
  },
];

export function getDefaultIcon(i_name?: string | null, i_type?: string | null): IDefaultIcon | null {
  return DEFAULT_ICONS.find(e => e.match(i_name, i_type))?.icon ?? null;
}
