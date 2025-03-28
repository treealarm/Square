-- Table: public.integro

-- DROP TABLE IF EXISTS public.integro;

CREATE TABLE IF NOT EXISTS public.integro
(
    id uuid NOT NULL,
    i_type character varying,
    i_name text,
    CONSTRAINT integro_pkey PRIMARY KEY (id)
);

CREATE INDEX idx_integro_i_type ON public.integro (i_type);
