import * as React from 'react';

import { useEffect, useState } from 'react';
import { useSelector } from "react-redux";
import * as EventsStore from '../store/EventsStates'
import Divider from '@mui/material/Divider';
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, TextField } from '@mui/material';

import { useAppDispatch } from '../store/configureStore';
import { DeepCopy, IEventDTO } from '../store/Marker';
import { DoFetch } from '../store/Fetcher';
import { ApiFileSystemRootString } from '../store/constants';


export function EventProperties() {

  const appDispatch = useAppDispatch();
  const selected_event: IEventDTO = useSelector((state: ApplicationState) => state?.eventsStates?.selected_event);

  const [images, setImg] = useState<Record<string, string>>({});

  const fetchImage = async (path: string) => {
    var temp = DeepCopy(images);
    const res = await DoFetch(ApiFileSystemRootString + "/GetFile/" + path);
    const imageBlob = await res.blob();
    const imageObjectURL = URL.createObjectURL(imageBlob);
    temp[path] = imageObjectURL;
    setImg(temp);
  };

  useEffect(() => {
    selected_event?.meta.not_indexed_props?.map((item, index) => {
      if (item.visual_type == 'image_fs') {
        fetchImage(item.str_val)
      }
    });
  }, [selected_event]);

  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 0
    }}>

      <List dense>
        {
          selected_event?.meta?.extra_props?.map((item, index) =>
            <ListItem key={index}>

              <TextField size="small"
                fullWidth
                id={item.prop_name} label={item.prop_name}
                value={item.str_val}
                inputProps={{ readOnly: true }} />
            </ListItem>
          )
        }
        <Divider><br /></Divider>
        {
          selected_event?.meta.not_indexed_props?.map((item, index) =>
            <div>

              <ListItem key={'image_fs' + index}>

                {item.visual_type == 'image_fs' ?
                  <div>
                    <Divider><br /></Divider>
                    <img
                      title={item.prop_name + "=" + item.str_val}
                      key={"img" + item.str_val}
                      src={images[item.str_val] != null ? images[item.str_val] : "svg/black_square.svg"}
                      style={{
                        border: 0,
                        padding: 0,
                        margin: 0,
                        width: '100%',
                        height: '100%',
                        objectFit: 'fill'
                      }} />
                  </div>
                  :
                  <TextField size="small"
                    fullWidth
                    id={item.prop_name} label={item.prop_name}
                    value={item.str_val}
                    inputProps={{ readOnly: true }} />
                }
              </ListItem>
            </div>
          )
        }
      </List>
    </Box>
  );
}