CREATE TABLE IF NOT EXISTS public.objects
(
    id UUID PRIMARY KEY,               -- вместо ObjectId будем хранить Guid
    parent_id UUID NULL,               -- ссылка на родителя
    owner_id UUID NULL,                -- ссылка на владельца
    name TEXT NOT NULL                 -- имя объекта
);

-- Индекс по parent_id
CREATE INDEX IF NOT EXISTS idx_objects_parent_id
    ON public.objects(parent_id);

-- Индекс по owner_id
CREATE INDEX IF NOT EXISTS idx_objects_owner_id
    ON public.objects(owner_id);

-- Составной индекс (id + owner_id), аналог как у тебя в Mongo
CREATE INDEX IF NOT EXISTS idx_objects_id_owner_id
    ON public.objects(id, owner_id);
