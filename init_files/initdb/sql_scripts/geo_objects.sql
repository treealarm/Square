CREATE EXTENSION IF NOT EXISTS postgis;
-- Таблица геообъектов
CREATE TABLE IF NOT EXISTS public.geo_objects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    figure geometry(GEOMETRY, 4326) NOT NULL, -- хранит Point, Polygon и т.д.
    radius double precision,
    zoom_level text
);

-- Индекс по zoom_level (btree)
CREATE INDEX IF NOT EXISTS idx_geo_objects_zoom_level 
    ON public.geo_objects(zoom_level);

-- Индекс для гео-запросов (gist по figure)
CREATE INDEX IF NOT EXISTS idx_geo_objects_figure_gist 
    ON public.geo_objects USING gist (figure);
