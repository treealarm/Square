CREATE TABLE IF NOT EXISTS public.diagrams (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    dgr_type TEXT NOT NULL,
    geometry JSONB,
    region_id TEXT,
    background_img TEXT
);
