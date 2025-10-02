CREATE TABLE  IF NOT EXISTS public.diagram_types (
    id uuid PRIMARY KEY,
    name text NOT NULL,
    src text,
    CONSTRAINT uq_diagram_types_name UNIQUE (name)
);

CREATE TABLE IF NOT EXISTS public.diagram_type_regions (
    diagram_type_id uuid NOT NULL REFERENCES diagram_types(id) ON DELETE CASCADE,
    region_key text NOT NULL,
    geometry jsonb NOT NULL,
    styles jsonb,
    PRIMARY KEY (diagram_type_id, region_key)
);

CREATE INDEX idx_diagram_types_name ON public.diagram_types(name);
