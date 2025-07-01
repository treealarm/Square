/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
import { useEffect, useState } from "react";
import { Card, CardMedia, Typography, Tooltip, Box } from "@mui/material";
import { IEventDTO } from "../store/Marker";
import { DoFetch } from "../store/Fetcher";
import { ApiFileSystemRootString } from "../store/constants";

export function EventCard({
  item,
  onSelect,
}: {
  item: any;
  onSelect: (event: IEventDTO | null) => void;
  }) {

  const [imageSrc, setImageSrc] = useState<string>("svg/black_square.svg");

  useEffect(() => {
    let isMounted = true;

    const loadImage = async () => {
      try {
        console.log("loadImage", item.prop.str_val);
        const res = await DoFetch(ApiFileSystemRootString + "/GetFile/" + item.prop.str_val);
        const imageBlob = await res.blob();
        const imageObjectURL = URL.createObjectURL(imageBlob);
        if (isMounted)
          setImageSrc(imageObjectURL);
      } catch (error) {
        console.error("Load error:", error);
      }
    };

    loadImage();

    return () => {
      isMounted = false;
      // Можно добавить освобождение URL.createObjectURL
      if (imageSrc.startsWith("blob:")) {
        URL.revokeObjectURL(imageSrc);
      }
    };
  }, [item?.prop?.str_val]);

  useEffect(() => {
    // This function will be called only once, after the component mounts
    console.log('Component is now visible for the first time!');
    // Perform any setup or data fetching here
  }, []);

  return (
    <Box>
      <Card
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          overflow: "hidden",
          aspectRatio: "16/9",
          cursor: "pointer",
          border: "2px solid transparent",
          "&:hover": { border: "2px solid #1976d2" },
        }}
        onClick={() => onSelect?.(item.event)}
      >
        <Tooltip
          arrow
          title={
            <div>
              {item.event.extra_props
                ?.filter((prop) => prop !== item.prop)
                .map((prop, idx) => (
                  <Typography key={idx} variant="body2">
                    {prop.prop_name}: {prop.str_val}
                  </Typography>
                )) || <Typography variant="body2">Нет доп. данных</Typography>}
            </div>
          }
        >
          <CardMedia
            component="img"
            image={imageSrc || "svg/black_square.svg"}
            alt={item.prop.prop_name}
            sx={{
              width: "100%",
              height: "100%",
              objectFit: "contain",
            }}
          />
        </Tooltip>
      </Card>
    </Box>
  );
}
