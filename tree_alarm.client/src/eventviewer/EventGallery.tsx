/* eslint-disable no-unused-vars */
import { useState } from "react";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { IEventDTO } from "../store/Marker";
import { Box, Grid } from "@mui/material";
import { EventCard } from "./EventCard"; // Не забудь создать этот файл

export function EventGallery({ onSelect }: { onSelect: (event: IEventDTO | null) => void }) {
  const events: IEventDTO[] = useSelector(
    (state: ApplicationState) => state?.eventsStates?.events
  ) ?? [];

  const cols = 3;

  const imageItems = events.flatMap((event) =>
    event.extra_props
      ?.filter((item) => item.visual_type === "image_fs")
      .map((item) => ({
        event,
        prop: item,
      })) || []
  );

  return (
    <Box sx={{ width: "99%", height: "80vh", overflowY: "auto", p: 2 }}>
      <Grid container spacing={2} columns={cols}>
        {imageItems.map((item, index) => (
          <Grid item key={item.prop.str_val} xs={1}>
            <EventCard item={item} onSelect={onSelect} />
          </Grid>
        ))}
      </Grid>

    </Box>
  );
}
