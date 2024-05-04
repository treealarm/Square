import * as React from 'react';
import { useState } from 'react';
import Box from '@mui/material/Box';

interface RectangleProps {
  initialX: number;
  initialY: number;
}

const DrawRectangle = (props:RectangleProps) => {
  const [pos, setPos] = useState({ x: props.initialX, y: props.initialY });
  const [isDragging, setIsDragging] = useState(false);
  const [startOffset, setStartOffset] = useState({ x: 0, y: 0 });

  const handleMouseDown = (event:any) => {
    setIsDragging(true);
    const { clientX, clientY } = event;
    const offsetX = clientX - pos.x;
    const offsetY = clientY - pos.y;
    setStartOffset({ x: offsetX, y: offsetY });
  };

  const handleMouseMove = (event:any) => {
    if (!isDragging) return;
    const { clientX, clientY } = event;
    setPos({
      x: clientX - startOffset.x,
      y: clientY - startOffset.y,
    });
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  return (
    <Box
      sx={{
        position: 'absolute',
        width: '100px',
        height: '100px',
        backgroundColor: 'blue',
        left: pos.x + 'px',
        top: pos.y + 'px',
        cursor: 'pointer',
      }}
      onMouseDown={handleMouseDown}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
    />
  );
};

export default DrawRectangle;