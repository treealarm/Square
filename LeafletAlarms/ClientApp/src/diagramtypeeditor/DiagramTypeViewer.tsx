import * as React from 'react';
import { useEffect } from 'react';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp, IDiagramCoord, IDiagramTypeDTO } from '../store/Marker';

import { useAppDispatch } from '../store/configureStore';
import * as DiagramTypeStore from '../store/DiagramTypeStates';
import { useState } from 'react';
import { Box, ButtonGroup, IconButton } from '@mui/material';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import { SelectChangeEvent } from '@mui/material/Select';


export default function DiagramTypeViewer() {

  const appDispatch = useAppDispatch();
  const [zoom, setZoom] = useState(1.0);

  const diagramType: IDiagramTypeDTO = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);
 

  var paper_width = 1000;
  var paper_height = 1000;

  useEffect(
    () => {
      //appDispatch<any>(DiagramTypeStore.fetchDiagramTypeByName(""));
    }, []);


  const handleChange = (event: SelectChangeEvent) => {

  };

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {

  };

  const handleClickRegion = (e: React.MouseEvent<HTMLDivElement>) => {

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
          onClick={handleClick}
          sx={{// Paper
            position: 'absolute',
            border: 0,
            padding: 0,
            margin: 0,
            top: '0px', // Сдвиг от верхнего края
            left: '65px', // Сдвиг от левого края
            height: paper_height * zoom + 'px',
            width: paper_width * zoom + 'px',
            backgroundColor: 'lightgray',
          }}>

          <Box
            key={"box green"}
            sx={{// Paper
              border: 0,
              width: 'fit-content', // Установите ширину равной содержимому
              height: 'fit-content', // Установите высоту равной содержимому
              position: 'relative',
            }}>
          <img
            key={"img" + diagramType?.id}
            src={diagramType?.src}
            style={{
              border: 0,
              padding: 0,
              margin: 0,
              //width: '100%',
             // height: '100%',
             // objectFit: 'fill'
            }}/>

            {
              diagramType?.regions?.map((region, index) =>
                < Box
                  onClick={handleClickRegion//() => { selectItem(diagram?.id) }
                  }
                  key={index.toString() + "boxitem"}
                  sx={{
                    position: 'absolute',
                    top: region.geometry.top * 100 + '%',
                    left: region.geometry.left * 100 + '%',
                    width: region.geometry.width * 100 + '%',
                    height: region.geometry.height * 100 + '%',
                    backgroundColor: 'rgba(0, 255, 0, 0.5)',
                    '&:hover': {
                      cursor: 'pointer'
                    },
                  }} />               
              )
            }

          </Box>
        </Box>
      </Box>

      <ButtonGroup variant="contained" orientation="vertical"
        sx={{ position: 'absolute', left: '5px', backgroundColor: 'lightgray' }}>
        <IconButton
          onClick={(e: any) => setZoom(zoom + 0.1)}>
          <ZoomInIcon fontSize="inherit"></ZoomInIcon>
        </IconButton>

        <IconButton
          onClick={(e: any) => setZoom(zoom - 0.1)}>
          <ZoomOutIcon fontSize="inherit" />
        </IconButton>
        <IconButton
          onClick={(e: any) => setZoom(1)}>
          <RestartAltIcon fontSize="inherit" />
        </IconButton>

      </ButtonGroup>

    </Box>
  );
}