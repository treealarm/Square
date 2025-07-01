import { useEffect, useRef, useState } from "react";
import { Card, CardMedia, Typography, Tooltip, Box } from "@mui/material";
import { useIntersectionObserver } from "./useIntersectionObserver"; // путь к хуку

export function EventCard({ item, onSelect, images, setImages }: any) {
  const ref = useRef<HTMLDivElement>(null);
  const isVisible = useIntersectionObserver(ref);

  useEffect(() => {
    const loadImage = async () => {
      if (!isVisible || images[item.prop.str_val]) return;

      try {
        const res = await fetch("/api/FileSystem/GetFile/" + item.prop.str_val);
        const blob = await res.blob();
        const url = URL.createObjectURL(blob);
        setImages((prev: any) => ({ ...prev, [item.prop.str_val]: url }));
      } catch (error) {
        console.error("Ошибка загрузки:", error);
      }
    };

    loadImage();
  }, [isVisible, item.prop.str_val, images, setImages]);

  return (
    <Box ref={ref}>
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
    </Box>
  );
}
