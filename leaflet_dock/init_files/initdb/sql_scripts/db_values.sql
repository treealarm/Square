CREATE TABLE IF NOT EXISTS public.db_values (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_id uuid NOT NULL,
    name character varying NOT NULL,
    value jsonb
);

-- Индексы
CREATE INDEX IF NOT EXISTS idx_db_values_owner_id ON public.db_values (owner_id);
CREATE INDEX IF NOT EXISTS idx_db_values_name ON public.db_values (name);
-- Если предполагаются запросы по значению
-- CREATE INDEX IF NOT EXISTS idx_db_values_value_jsonb ON public.db_values USING GIN (value);
