/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable no-unused-vars */
import { useEffect, useState, useRef } from "react";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { DeepCopy, IEventDTO } from "../store/Marker";
import { DoFetch } from "../store/Fetcher";
import { ApiFileSystemRootString } from "../store/constants";
import { Box, Card, CardMedia, Grid } from "@mui/material";
import { Tooltip } from "@mui/material";

export function EventGallery({ rows = 3, onSelect }: { rows?: number; onSelect?: (event?: IEventDTO|null) => void }) {
  const events: IEventDTO[] = useSelector(
    (state: ApplicationState) => state?.eventsStates?.events
  ) ?? [];

  const [images, setImages] = useState<Record<string, string>>({});
  const [cols, setCols] = useState(3);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const loadedImages = new Set(Object.keys(images)); // »спользуем Set дл€ быстрого поиска уже загруженных картинок

    const fetchImage = async (path: string) => {
      if (loadedImages.has(path)) return; // ѕропускаем загрузку, если уже загружено

      try {
        const res = await DoFetch(ApiFileSystemRootString + "/GetFile/" + path);
        const imageBlob = await res.blob();
        const imageObjectURL = URL.createObjectURL(imageBlob);

        setImages((prev) => ({ ...prev, [path]: imageObjectURL }));
      } catch (error) {
        console.error("ќшибка загрузки изображени€:", error);
      }
    };

    events.forEach((event) => {
      event.extra_props?.forEach((item) => {
        if (item.visual_type === "image_fs") {
          fetchImage(item.str_val);
        }
      });
    });
  }, [events]); // ”брали images из зависимостей, чтобы избежать зацикливани€


  useEffect(() => {
    const updateCols = () => {
      if (containerRef.current) {
        const containerWidth = containerRef.current.offsetWidth;
        const maxCardHeight = containerWidth / (16 / 9) / rows;
        const estimatedCols = Math.floor(containerWidth / (maxCardHeight * (16 / 9)));

        setCols(Math.max(estimatedCols, 1));
      }
    };

    updateCols();
    window.addEventListener("resize", updateCols);
    return () => window.removeEventListener("resize", updateCols);
  }, [rows]);

  let imageItems = events.flatMap((event) =>
    event.extra_props
      ?.filter((item) => item.visual_type === "image_fs")
      .map((item) => ({
        event,
        prop: item,
      })) || []
  );

  const maxItems = rows * cols;
  while (imageItems.length < maxItems) {
    imageItems.push(null);
  }
  imageItems = imageItems.slice(0, maxItems);

  return (
    <Box ref={containerRef} sx={{ width: "100%", overflow: "auto", p: 2 }}>
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
                cursor: item ? "pointer" : "default",
                border: item ? "2px solid transparent" : "2px dashed rgba(0,0,0,0.2)",
                "&:hover": item ? { border: "2px solid #1976d2" } : {},
              }}
              onClick={() => item && onSelect?.(item.event)}
            >
              {item ? (
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
              ) : (
                <Box sx={{ width: "100%", height: "100%", backgroundColor: "rgba(0,0,0,0.05)" }} />
              )}
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
