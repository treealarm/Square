import React from "react";

interface SnapshotSimpleViewerProps {
  imageSrc?: string | null;
}

export const SnapshotSimpleViewer: React.FC<SnapshotSimpleViewerProps> = ({
  imageSrc
}) => {
  if (!imageSrc) return null;

  return (
    <img
      src={`${imageSrc}?t=${Date.now()}`}
      alt="Snapshot"
      style={{
        display: "block",
        width: "100%",
        height: "auto",
        objectFit: "contain"
      }}
    />
  );
};
