CREATE TABLE IF NOT EXISTS public.rights (
    object_id uuid NOT NULL,
    role varchar NOT NULL,
    value integer NOT NULL,
    PRIMARY KEY (object_id, role)
);

-- индексы
CREATE INDEX IF NOT EXISTS idx_rights_object_id ON public.rights(object_id);
CREATE INDEX IF NOT EXISTS idx_rights_role ON public.rights(role);
CREATE INDEX IF NOT EXISTS idx_rights_value ON public.rights(value);
