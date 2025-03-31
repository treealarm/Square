-- Table: public.integro

-- DROP TABLE IF EXISTS public.integro;
-- ������� ������������ ������� � id , 
-- ��� ���� ���������� i_type (cam, main...) � 
-- ��� ����������� i_name (app_id)
CREATE TABLE IF NOT EXISTS public.integro
(
    id uuid NOT NULL,
    i_type character varying,
    i_name text,
    CONSTRAINT integro_pkey PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS idx_integro_i_type ON public.integro (i_type);

-- ������ ������� �����
CREATE TABLE IF NOT EXISTS public.integro_types
(
    i_type character varying PRIMARY KEY
);


-- id ������ ���������� id, child_i_type ���� ������� ����� ������� ������ ���.
CREATE TABLE IF NOT EXISTS public.integro_type_children
(
    i_type character varying,
    child_i_type character varying,
    CONSTRAINT pk_i_type_child PRIMARY KEY (i_type, child_i_type)
);

CREATE INDEX IF NOT EXISTS idx_i_type ON public.integro_type_children (i_type);
