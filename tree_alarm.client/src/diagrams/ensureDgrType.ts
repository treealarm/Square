import { DeepCopy, IDiagramContentDTO, IDiagramTypeDTO } from '../store/Marker';
import * as DiagramTypeStore from '../store/DiagramTypeStates';
import * as DiagramsStore from '../store/DiagramsStates';
import { AppDispatch } from '../store/configureStore';

// Looks up a dgr_type by name (first in the already-loaded content, then on the server),
// creating it if it doesn't exist anywhere yet, and makes sure it's present in
// cur_diagram_content.dgr_types so it renders immediately. Used to auto-assign a shared
// "camera" look to a camera dropped onto a diagram for the first time.
export async function ensureDgrType(
  appDispatch: AppDispatch,
  content: IDiagramContentDTO,
  typeDef: { name: string; src: string }
): Promise<string> {
  const existingLocal = content.dgr_types?.find(t => t.name === typeDef.name);
  if (existingLocal) {
    return existingLocal.name;
  }

  let dgrType: IDiagramTypeDTO | null = null;
  try {
    const byName = await appDispatch(DiagramTypeStore.fetchDiagramTypeByName(typeDef.name)).unwrap();
    dgrType = byName?.dgr_types?.[0] ?? null;
  } catch {
    dgrType = null;
  }

  if (!dgrType) {
    const created = await appDispatch(
      DiagramTypeStore.updateDiagramTypes([{ id: null, name: typeDef.name, src: typeDef.src, regions: [] }])
    ).unwrap();
    dgrType = created?.dgr_types?.[0] ?? null;
  }

  if (dgrType) {
    const updatedContent = DeepCopy(content);
    if (updatedContent) {
      updatedContent.dgr_types = [...(updatedContent.dgr_types ?? []), dgrType];
      appDispatch(DiagramsStore.set_diagram_content_locally(updatedContent));
    }
  }

  return dgrType?.name ?? typeDef.name;
}
