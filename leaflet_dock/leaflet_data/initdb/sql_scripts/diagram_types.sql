-- =====================================================================
-- SCHEMA: public
-- MODULE: States
-- DESCRIPTION: ������� ��� �������� ��������� ��������, �������� ��������� � �������� �������
-- =====================================================================


-- ================================================================
-- TABLE: object_states
-- ================================================================
CREATE TABLE IF NOT EXISTS public.object_states (
    id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
    "timestamp" timestamp with time zone NOT NULL DEFAULT now()
);

-- ================================================================
-- TABLE: object_state_values
-- One-to-many: object_state -> states
-- ================================================================
CREATE TABLE IF NOT EXISTS public.object_state_values (
    id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
    object_id uuid NOT NULL REFERENCES public.object_states(id) ON DELETE CASCADE,
    state character varying NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_object_state_values_object_id 
    ON public.object_state_values (object_id);


-- ================================================================
-- TABLE: object_state_descriptions
-- ���������� ��������� ���������
-- ================================================================
CREATE TABLE IF NOT EXISTS public.object_state_descriptions (
    id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
    alarm boolean NOT NULL DEFAULT false,
    state character varying NOT NULL UNIQUE,
    state_descr text,
    state_color character varying
);

-- ������ ��� ���������� �� (state, alarm)
CREATE INDEX IF NOT EXISTS idx_state_descriptions_state_alarm 
    ON public.object_state_descriptions (state, alarm);


-- ================================================================
-- TABLE: alarm_states
-- �������� �������� ��������� ������ ��� ��������
-- ================================================================
CREATE TABLE IF NOT EXISTS public.alarm_states (
    id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
    alarm boolean NOT NULL
);

-- ������� ����� �� �����������, ���� �������������� ������ �� id (PK)


-- =====================================================================
-- END OF MODULE: States
-- =====================================================================
