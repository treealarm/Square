--  ./pg_dump -U keycloak -d MapStore --create --clean --schema-only --no-owner --no-privileges -f D:\TESTS\Leaflet\DbLayer\sql\create.sql
-- PostgreSQL database dump
--

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: event_props; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.event_props (
    prop_name character varying(255),
    str_val text,
    visual_type character varying(255),
    id uuid DEFAULT gen_random_uuid(),
    owner_id uuid
);


--
-- Name: events; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.events (
    object_id uuid,
    event_name character varying(255),
    event_priority integer,
    "timestamp" timestamp without time zone,
    id uuid DEFAULT gen_random_uuid()
);

-- ������ ��� ���������� �� ������� (����������� �������)
CREATE INDEX idx_events_timestamp ON public.events ("timestamp");

-- ������ ��� ������ �� event_name (���� ������������ �������� LIKE, ����� ����������� gin_trgm_ops)
CREATE INDEX idx_events_event_name ON public.events (event_name);

-- ������ ��� ���������� �� object_id (��� ������ � ��������)
CREATE INDEX idx_events_object_id ON public.events (object_id);

-- ������ ��� extra_props -> ����� �� prop_name � str_val
CREATE INDEX idx_event_props_prop_name_val ON public.event_props (owner_id, prop_name, str_val);

-- ������� ��� ���������� (���� ���������� ���� �� ������ �����)
CREATE INDEX idx_events_priority ON public.events (event_priority);
CREATE INDEX idx_events_timestamp_desc ON public.events ("timestamp" DESC);


--
-- PostgreSQL database dump complete
--

