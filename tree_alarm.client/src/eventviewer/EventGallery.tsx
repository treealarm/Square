/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import { useEffect, useState, useRef } from "react";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { IEventDTO } from "../store/Marker";
import { DoFetch } from "../store/Fetcher";
import { ApiFileSystemRootString } from "../store/constants";
import { Box, Card, CardMedia, Grid, Typography, Tooltip } from "@mui/material";

export function EventGallery({ onSelect }: { onSelect: (event: IEventDTO | null) => void }) {
  const events: IEventDTO[] = useSelector(
    (state: ApplicationState) => state?.eventsStates?.events
  ) ?? [];

  const [images, setImages] = useState<Record<string, string>>({});
  const containerRef = useRef<HTMLDivElement>(null);

  // Количество колонок задаём явно
  const cols = 3;

  useEffect(() => {
    const loadedImages = new Set(Object.keys(images));

    const fetchImage = async (path: string) => {
      if (loadedImages.has(path)) return;

      try {
        const res = await DoFetch(ApiFileSystemRootString + "/GetFile/" + path);
        const imageBlob = await res.blob();
        const imageObjectURL = URL.createObjectURL(imageBlob);

        setImages((prev) => ({ ...prev, [path]: imageObjectURL }));
      } catch (error) {
        console.error("Ошибка загрузки изображения:", error);
      }
    };

    events.forEach((event) => {
      event.extra_props?.forEach((item) => {
        if (item.visual_type === "image_fs") {
          fetchImage(item.str_val);
        }
      });
    });
  }, [events]);

  // Собираем список картинок
  const imageItems = events.flatMap((event) =>
    event.extra_props
      ?.filter((item) => item.visual_type === "image_fs")
      .map((item) => ({
        event,
        prop: item,
      })) || []
  );

  return (
    <Box ref={containerRef} sx={{ width: "99%", height: "80vh", overflowY: "auto", p: 2 }}>
      <Grid container spacing={2} columns={cols}>
        {imageItems.map((item, index) => (
          <Grid item key={index} xs={1}>
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
              onClick={() => item && onSelect?.(item.event)}
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
                  image={images[item.prop.str_val] || "svg/black_square.svg"}
                  alt={item.prop.prop_name}
                  sx={{
                    width: "100%",
                    height: "100%",
                    objectFit: "contain",
                  }}
                />
              </Tooltip>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
