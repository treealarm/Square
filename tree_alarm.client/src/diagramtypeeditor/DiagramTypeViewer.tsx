import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { IDiagramTypeDTO } from '../store/Marker';

import { useState } from 'react';
import { Box, ButtonGroup, IconButton, Tooltip } from '@mui/material';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import RestartAltIcon from '@mui/icons-material/RestartAlt';


export default function DiagramTypeViewer() {

  const [zoom, setZoom] = useState(1.0);

  const diagramType: IDiagramTypeDTO = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);


  var paper_width = 1000;
  var paper_height = 1000;

  const [imageSize, setImageSize] = useState({ width: 0, height: 0 });

  const handleImageLoad = (event: any) => {
    const { naturalWidth, naturalHeight } = event.target;
    setImageSize({ width: naturalWidth, height: naturalHeight });
  };

  if (diagramType == null) {
    return null;
  }


  return (
    <Box
      key={"box top"}
      sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        border: 0,
        padding: 0,
        margin: 0,
        position: 'relative',
      }}
    >
      <Box
        key={"box transparent"}
        sx={{
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          backgroundColor: 'gray',
          border: 0,
          padding: 0,
          margin: 0,
          position: 'relative',
          overflow: 'auto'
        }}
      >
        <Box
          key={"box yellow"}
          bgcolor="primary.light" 
          sx={{// Paper
            position: 'absolute',
            border: 0,
            padding: 0,
            margin: 0,
            top: '0px', // Сдвиг от верхнего края
            left: '65px', // Сдвиг от левого края
            height: paper_height * zoom + 'px',
            width: paper_width * zoom + 'px',
            
          }}>

          <Box
            key={"box green"}
            sx={{// Paper
              border: 0,
              width: 'fit-content', // Установите ширину равной содержимому
              height: 'fit-content', // Установите высоту равной содержимому
              position: 'relative',
            }}>
          </Box>
          <img
            key={"img" + diagramType?.id}
            src={diagramType?.src}
            onLoad={handleImageLoad}
            style={{
              width: zoom * imageSize.width + 'px', // Ширина в процентах от реальной ширины
              height: zoom * imageSize.height + 'px', // Высота в процентах от реальной высоты
              // Другие стили
            }} />

          {
            diagramType?.regions?.map((region, index) =>
              < Box
                key={index.toString() + "boxitem"}
                sx={{
                  position: 'absolute',
                  left: imageSize.width * region.geometry.left * zoom + 'px',
                  top: imageSize.height * region.geometry.top * zoom + 'px',

                  width: imageSize.width * region.geometry.width * zoom + 'px',
                  height: imageSize.height * region.geometry.height * zoom + 'px',
                  backgroundColor: 'rgba(0, 255, 0, 0.5)',
                  '&:hover': {
                    cursor: 'pointer'
                  },
                }} />
            )
          }


        </Box>
      </Box>

      <Tooltip title={"Zoom " + zoom.toFixed(2)}>
        <ButtonGroup variant="contained" orientation="vertical"
          sx={{ position: 'absolute', left: '5px', backgroundColor: 'lightgray' }}>

          <IconButton
            onClick={() => setZoom(zoom + 0.1)}>
            <ZoomInIcon fontSize="inherit"></ZoomInIcon>
          </IconButton>

          <IconButton
            onClick={() => setZoom(zoom - 0.1)}>
            <ZoomOutIcon fontSize="inherit" />
          </IconButton>
          <IconButton
            onClick={() => setZoom(1)}>
            <RestartAltIcon fontSize="inherit" />
          </IconButton>

        </ButtonGroup>
      </Tooltip>
    </Box>
  );
}