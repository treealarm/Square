import { IUpdateIntegroObjectDTO, IObjProps } from "../store/Marker";
import * as IntegroStore from '../store/IntegroStates';

export const createObjectsFromBrowserFile = (
  file: File,
  parentId: string | null,
  appDispatch: any,
  i_name: string|null
) => {

  if (!i_name) {
    console.log("i_name is empty");
    return;
  }
  const reader = new FileReader();

  reader.onload = (event) => {
    try {
      const content = event.target?.result as string;
      const cameras: IUpdateIntegroObjectDTO[] = JSON.parse(content);

      cameras.forEach(cam => {
        // Добавляем parent_id и id
        const objProps: IObjProps = {
          ...cam.obj,
          id: null,
          parent_id: parentId
        };

        const newObj: IUpdateIntegroObjectDTO = {
          obj: objProps,
          integro:
          {
            i_name: i_name,
            i_type: cam.integro.i_type,
            id: cam.integro.id
          }
            
        };

        appDispatch(IntegroStore.updateIntegroObject(newObj));
      });
    } catch (e) {
      console.error('Error reading file', e);
    }
  };

  reader.readAsText(file);
};

