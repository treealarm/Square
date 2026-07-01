import { AppDispatch } from '../store/configureStore';
import * as ObjPropsStore from '../store/ObjPropsStates';
import * as DiagramsStore from '../store/DiagramsStates';
import * as GuiStore from '../store/GUIStates';
import { DeepCopy, IDiagramContentDTO, IDiagramDTO, IObjProps } from '../store/Marker';
import { fetchIntegroInfoByIds } from '../tree/integroInfo';
import { CAMERA_DGR_TYPE_NAME, CAMERA_ICON_PATH, getDefaultIcon } from '../tree/defaultIcons';
import { ensureDgrType } from './ensureDgrType';

const ICON_SIZE = 40;

// Creates a brand-new object owned by `ownerId` (owner_id links back, extra_props are a
// snapshot copy) and places it on the given diagram at `position` — shared by both the native
// drag-and-drop of the Properties panel's replica handle onto a diagram (DiagramViewer.tsx) and
// the editor's "attach to this diagram" action, instead of moving the source object itself.
export async function createDiagramReplicaOnDiagram(
  appDispatch: AppDispatch,
  ownerId: string,
  diagramId: string,
  position: { left: number; top: number },
  cur_diagram_content: IDiagramContentDTO | null
): Promise<string | null> {
  const owner = await ObjPropsStore.requestObjPropsById(ownerId);
  if (!owner) return null;

  const newObjProps: IObjProps = {
    id: null,
    name: owner.name,
    parent_id: diagramId,
    owner_id: ownerId,
    extra_props: DeepCopy(owner.extra_props) ?? [],
  };
  const created = await appDispatch(ObjPropsStore.updateObjProps(newObjProps)).unwrap();
  if (!created.id) return null;

  const dto: IDiagramDTO = {
    id: created.id,
    geometry: { ...position, width: ICON_SIZE, height: ICON_SIZE, rotation: 0 },
    dgr_type: null,
    region_id: null,
    background_img: null,
  };

  const [integroInfo] = await fetchIntegroInfoByIds([ownerId]).catch(() => []);
  const defaultIcon = getDefaultIcon(integroInfo?.i_name, integroInfo?.i_type);
  if (defaultIcon && cur_diagram_content) {
    dto.dgr_type = await ensureDgrType(appDispatch, cur_diagram_content, {
      name: CAMERA_DGR_TYPE_NAME,
      src: CAMERA_ICON_PATH,
    });
  }

  appDispatch(DiagramsStore.updateDiagrams([dto]));
  appDispatch(GuiStore.requestTreeUpdate());
  return created.id;
}
