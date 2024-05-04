import * as React from "react";
import { styled } from '@mui/material/styles';
import Button from '@mui/material/Button';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';

import { DoFetch } from "../store/Fetcher";
import { Tooltip } from '@mui/material';
import { ApiFileSystemRootString } from "../store/constants";

const VisuallyHiddenInput = styled('input')({
  clip: 'rect(0 0 0 0)',
  clipPath: 'inset(50%)',
  height: 1,
  overflow: 'hidden',
  position: 'absolute',
  bottom: 0,
  left: 0,
  whiteSpace: 'nowrap',
  width: 1,
});

interface FileUploadProps {
  onUploadSuccess: (data: any) => void; // Тип функции колбэка
  path: string; // Тип строки для пути
}

const FileUpload: React.FC<FileUploadProps> = ({ onUploadSuccess, path }) => {

  const handleFileChange = (event:any) => {
    handleUpload(event.target.files[0]);
  };

  const handleUpload = async (file: any) => {
    if (file) {
      const formData = new FormData();
      formData.append('file', file);
      try {

        var response = await DoFetch(ApiFileSystemRootString + "/upload_static?path=" + encodeURIComponent(path),
          {
            method: "POST",
            headers: {
              'Accept': 'text/plain'
            },
            body: formData
          });

        if (!response.ok) {
          console.log('Network response was not ok');
        }
        const data = await response.text();
        // Обработка успешного ответа
        console.log('Data received:', data);
        onUploadSuccess('static_files/' + path + '/' + file.name);
      } catch (error) {
        // Обработка ошибок
        console.error('Error fetching data:', error);
        onUploadSuccess(null);
      }
    }
  };

  return (
    <Tooltip title={"File upload"}>
      <Button
        component="label"
        role={undefined}
        variant="contained"
        tabIndex={-1}
        startIcon={<CloudUploadIcon />}        
      >
        <VisuallyHiddenInput type="file" onChange={handleFileChange} />
        </Button>
    </Tooltip>
  );
};

export default FileUpload;
