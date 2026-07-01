import { DoFetch } from '../store/Fetcher';
import { ApiIntegroRootString } from '../store/constants';

// Shared lookup for "is this object produced by an integration, and which one" — used
// wherever we need i_name/i_type to pick a default icon (Add Geo, drag-and-drop placement,
// the Properties panel's vms_rec-camera check).
export interface IIntegroInfo {
  i_name?: string;
  i_type?: string;
}

export async function fetchIntegroInfoByIds(ids: string[]): Promise<IIntegroInfo[]> {
  const response = await DoFetch(`${ApiIntegroRootString}/GetByIds`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(ids),
  });
  return response.ok ? response.json() : [];
}
