CREATE TABLE diagram_types (
    id uuid PRIMARY KEY,
    name text NOT NULL,
    src text,
    CONSTRAINT uq_diagram_types_name UNIQUE (name)
);

CREATE TABLE diagram_type_regions (
    id uuid PRIMARY KEY,
    diagram_type_id uuid NOT NULL REFERENCES diagram_types(id) ON DELETE CASCADE,
    region_key text NOT NULL,
    geometry jsonb NOT NULL,
    styles jsonb
);

CREATE INDEX idx_diagram_types_name ON diagram_types(name);
