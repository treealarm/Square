-- Table: public.integro

-- DROP TABLE IF EXISTS public.integro;
-- таблица соответствия объекта с id , 
-- его типа интеграции i_type (cam, main...) и 
-- его микромодуля i_name (app_id)
CREATE TABLE IF NOT EXISTS public.integro
(
    id uuid NOT NULL,
    i_type character varying,
    i_name  character varying,
    CONSTRAINT integro_pkey PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS idx_integro_i_type ON public.integro (i_type);

-- просто таблица типов
CREATE TABLE IF NOT EXISTS public.integro_types
(
    i_type character varying,
    i_name  character varying,
    CONSTRAINT pk_i_type_name PRIMARY KEY (i_type, i_name)
);


-- id просто уникальный id, child_i_type типы которые может создать данный тип.
CREATE TABLE IF NOT EXISTS public.integro_type_children
(
    i_type character varying,
    child_i_type character varying,
    i_name  character varying,
    CONSTRAINT pk_i_type_child PRIMARY KEY (i_type, i_name, child_i_type)
);

CREATE INDEX IF NOT EXISTS idx_i_type ON public.integro_type_children (i_type);
