-- Основная таблица для свойств объектов
CREATE TABLE IF NOT EXISTS public.properties (
    id uuid DEFAULT gen_random_uuid() PRIMARY KEY,
    prop_name character varying NOT NULL,
    str_val text,
    visual_type character varying,
    object_id uuid NOT NULL
);

-- Индекс для быстрых выборок всех свойств по объекту
CREATE INDEX IF NOT EXISTS idx_properties_object_id
    ON public.properties(object_id);

-- Индекс для поиска по имени свойства
CREATE INDEX IF NOT EXISTS idx_properties_prop_name
    ON public.properties(prop_name);

-- Составной индекс: имя + значение
-- ускоряет WHERE prop_name = ? AND str_val = ?
CREATE INDEX IF NOT EXISTS idx_properties_prop_name_str_val
    ON public.properties(prop_name, str_val);

-- (опционально) если нужен поиск по подстроке в str_val
-- требует расширение pg_trgm:
-- CREATE EXTENSION IF NOT EXISTS pg_trgm;
-- CREATE INDEX IF NOT EXISTS idx_properties_str_val_trgm
--   ON public.properties USING gin (str_val gin_trgm_ops);
