-- Основная таблица properties
CREATE TABLE IF NOT EXISTS public.properties (
    id uuid DEFAULT gen_random_uuid() PRIMARY KEY
);

-- Таблица для свойств объекта
CREATE TABLE IF NOT EXISTS public.properties_extra_props (
    id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
    prop_name character varying NOT NULL,
    str_val text,
    visual_type character varying,
    owner_id uuid NOT NULL REFERENCES public.properties(id) ON DELETE CASCADE
);

-- Индексы (минимум для owner_id, чтобы быстро доставать все свойства объекта)
CREATE INDEX IF NOT EXISTS idx_properties_extra_props_owner_id
    ON public.properties_extra_props(owner_id);

-- Если потом появится поиск по prop_name/str_val → можно будет добавить такие же индексы, как у event_props
