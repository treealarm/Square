import { createContext, useContext } from 'react';

// Diagram editing (drag-to-move, rotation handle, drop targets, Add/Delete Diagram, type
// selection) is only meant to happen inside the dedicated /_editdiagrams workspace — everywhere
// else (the main / view) these components render read-only/inert. Default false so existing
// call sites outside a <DiagramEditingContext.Provider> keep their new, safe default.
export const DiagramEditingContext = createContext(false);

export const useDiagramEditing = () => useContext(DiagramEditingContext);
