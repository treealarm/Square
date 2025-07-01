/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
import { Card, Box, Typography, CardMedia } from "@mui/material";
import { useState, useEffect, useRef } from "react";
import { Tooltip } from "@mui/material";
import { DoFetch } from "../store/Fetcher";
import { IEventDTO } from "../store/Marker";
import { ApiFileSystemRootString } from "../store/constants";

export function EventCard({
  item,
  onSelect,
}: {
  item: any;
  onSelect: (event: IEventDTO | null) => void;
}) {
  const [imageSrc, setImageSrc] = useState<string>("svg/black_square.svg");
  const [hasLoaded, setHasLoaded] = useState(false);
  const cardRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!item?.prop?.str_val) return;

    let observer: IntersectionObserver | null = null;
    let isMounted = true;

    const onIntersect: IntersectionObserverCallback = (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting && !hasLoaded) {
          loadImage();
        }
      });
    };

    const loadImage = async () => {
      try {
        console.log("loadImage", item.prop.str_val);
        const res = await DoFetch(ApiFileSystemRootString + "/GetFile/" + item.prop.str_val);
        const imageBlob = await res.blob();
        const imageObjectURL = URL.createObjectURL(imageBlob);
        if (isMounted) {
          setImageSrc(imageObjectURL);
          setHasLoaded(true);
        }
      } catch (error) {
        console.error("Ошибка загрузки:", error);
      }
    };

    if (cardRef.current) {
      observer = new IntersectionObserver(onIntersect, {
        root: null,
        rootMargin: "100px", // можно подстраивать, чтоб загружать чуть заранее
        threshold: 0.1,
      });
      observer.observe(cardRef.current);
    }

    return () => {
      isMounted = false;
      if (observer && cardRef.current) {
        observer.unobserve(cardRef.current);
      }
      if (imageSrc.startsWith("blob:")) {
        URL.revokeObjectURL(imageSrc);
      }
    };
  }, [item?.prop?.str_val, hasLoaded]);

  if (!item || !item.prop) {
    return (
      <Card
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          overflow: "hidden",
          aspectRatio: "16/9",
          border: "2px dashed rgba(0,0,0,0.2)",
        }}
      >
        <Box sx={{ width: "100%", height: "100%", backgroundColor: "rgba(0,0,0,0.05)" }} />
      </Card>
    );
  }

  return (
    <Card
      ref={cardRef}
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
      <Tooltip
        arrow
        title={
          <div>
            {item.event.extra_props
              ?.filter((prop) => prop !== item.prop)
              .map((prop, index) => (
                <Typography key={index} variant="body2">
                  {prop.prop_name}: {prop.str_val}
                </Typography>
              )) || <Typography variant="body2">Нет дополнительных данных</Typography>}
          </div>
        }
      >
        <CardMedia
          component="img"
          image={imageSrc}
          alt={item.prop.prop_name}
          sx={{
            width: "100%",
            height: "100%",
            objectFit: "contain",
          }}
        />
      </Tooltip>
    </Card>
  );
}
