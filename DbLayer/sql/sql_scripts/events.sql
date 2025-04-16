--  ./pg_dump -U keycloak -d MapStore --create --clean --schema-only --no-owner --no-privileges -f D:\TESTS\Leaflet\DbLayer\sql\create.sql
-- PostgreSQL database dump
--

--
-- Name: event_props; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE IF NOT EXISTS public.event_props (
    prop_name character varying,
    str_val text,
    visual_type character varying,
    id uuid DEFAULT gen_random_uuid(),
    owner_id uuid
);


--
-- Name: events; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE IF NOT EXISTS public.events (
    object_id uuid,
    event_name character varying,
    event_priority integer,
    "timestamp" timestamp without time zone,
    id uuid DEFAULT gen_random_uuid()
);

-- ������ ��� ���������� �� ������� (����������� �������)
CREATE INDEX IF NOT EXISTS idx_events_timestamp ON public.events ("timestamp");

-- ������ ��� ������ �� event_name (���� ������������ �������� LIKE, ����� ����������� gin_trgm_ops)
CREATE INDEX IF NOT EXISTS idx_events_event_name ON public.events (event_name);

-- ������ ��� ���������� �� object_id (��� ������ � ��������)
CREATE INDEX IF NOT EXISTS idx_events_object_id ON public.events (object_id);

-- ������ ��� extra_props -> ����� �� prop_name � str_val
CREATE INDEX IF NOT EXISTS idx_event_props_prop_name_val ON public.event_props (owner_id, prop_name, str_val);

-- ������� ��� ���������� (���� ���������� ���� �� ������ �����)
CREATE INDEX IF NOT EXISTS idx_events_priority ON public.events (event_priority);
CREATE INDEX IF NOT EXISTS idx_events_timestamp_desc ON public.events ("timestamp" DESC);



