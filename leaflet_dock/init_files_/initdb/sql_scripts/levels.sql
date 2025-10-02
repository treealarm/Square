CREATE TABLE IF NOT EXISTS public.levels (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    zoom_level TEXT NOT NULL,
    zoom_min INT NOT NULL,
    zoom_max INT NOT NULL
);