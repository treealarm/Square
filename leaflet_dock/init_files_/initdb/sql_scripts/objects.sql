CREATE TABLE IF NOT EXISTS public.objects
(
    id UUID PRIMARY KEY,               -- ������ ObjectId ����� ������� Guid
    parent_id UUID NULL,               -- ������ �� ��������
    owner_id UUID NULL,                -- ������ �� ���������
    name TEXT NOT NULL                 -- ��� �������
);

-- ������ �� parent_id
CREATE INDEX IF NOT EXISTS idx_objects_parent_id
    ON public.objects(parent_id);

-- ������ �� owner_id
CREATE INDEX IF NOT EXISTS idx_objects_owner_id
    ON public.objects(owner_id);

-- ��������� ������ (id + owner_id), ������ ��� � ���� � Mongo
CREATE INDEX IF NOT EXISTS idx_objects_id_owner_id
    ON public.objects(id, owner_id);
