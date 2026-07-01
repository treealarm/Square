// Shared HTML5 drag-and-drop mime type for dragging a tree item (by marker id) onto the
// map or a diagram to place it there.
export const TREE_MARKER_DRAG_TYPE = 'application/x-square-marker-id';

// Dragging this (from the Properties panel's replica handle) onto the map creates a brand-new
// object with owner_id = the dragged id, instead of moving the dragged object's own geometry
// like TREE_MARKER_DRAG_TYPE does.
export const OBJECT_REPLICA_DRAG_TYPE = 'application/x-square-replica-owner-id';

const PIN_SVG = '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">' +
  '<path fill="#1976d2" stroke="white" stroke-width="1" ' +
  'd="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5A2.5 2.5 0 1 1 12 6.5a2.5 2.5 0 0 1 0 5z"/></svg>';

let ghostImg: HTMLImageElement | null = null;

// A small pin, not a screenshot of the whole tree row, as the native drag preview — set via
// dataTransfer.setDragImage on dragstart. Lazily created once and reused for every drag;
// kept off-screen (not display:none, which some browsers refuse to use as a drag image).
export function getDragGhostImage(): HTMLImageElement {
  if (ghostImg) {
    return ghostImg;
  }
  ghostImg = document.createElement('img');
  ghostImg.src = `data:image/svg+xml;utf8,${encodeURIComponent(PIN_SVG)}`;
  ghostImg.style.position = 'fixed';
  ghostImg.style.top = '-1000px';
  ghostImg.style.width = '24px';
  ghostImg.style.height = '24px';
  document.body.appendChild(ghostImg);
  return ghostImg;
}
