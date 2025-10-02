CREATE TABLE IF NOT EXISTS public.objects
(
    id UUID PRIMARY KEY,               -- вместо ObjectId будем хранить Guid
    parent_id UUID NULL,               -- ссылка на родител¤
    owner_id UUID NULL,                -- ссылка на владельца
    name TEXT NOT NULL                 -- им¤ объекта
);

-- »ндекс по parent_id
CREATE INDEX IF NOT EXISTS idx_objects_parent_id
    ON public.objects(parent_id);

-- »ндекс по owner_id
CREATE INDEX IF NOT EXISTS idx_objects_owner_id
    ON public.objects(owner_id);

-- —оставной индекс (id + owner_id), аналог как у теб¤ в Mongo
CREATE INDEX IF NOT EXISTS idx_objects_id_owner_id
    ON public.objects(id, owner_id);
