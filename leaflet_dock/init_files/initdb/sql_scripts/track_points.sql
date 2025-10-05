CREATE EXTENSION IF NOT EXISTS postgis;
-- Таблица трек-точек
CREATE TABLE IF NOT EXISTS public.track_points (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    object_id UUID NULL,
    timestamp TIMESTAMPTZ NOT NULL,

    -- геометрия фигуры (Point, Polygon, LineString)
    figure geometry(GEOMETRY, 4326) NOT NULL,
    radius double precision,
    zoom_level text,

    -- метаданные
    extra_props jsonb
);

-- индекс по object_id для фильтрации по объекту
CREATE INDEX IF NOT EXISTS idx_track_points_object_id ON public.track_points(object_id);

-- Индекс для быстрого поиска по времени
CREATE INDEX IF NOT EXISTS idx_track_points_ts ON public.track_points(timestamp);

-- GIN-индекс для быстрого поиска по jsonb свойствам
CREATE INDEX IF NOT EXISTS idx_track_points_extra_props ON public.track_points USING gin (extra_props);

-- Пример поиска по свойству в jsonb:
-- SELECT * FROM track_points
-- WHERE extra_props @> '[{"prop_name": "track_name", "str_val": "sad"}]';

-- Индекс для гео-запросов (например ближайшие точки)
CREATE INDEX IF NOT EXISTS idx_track_points_figure_gist ON public.track_points USING gist (figure);
