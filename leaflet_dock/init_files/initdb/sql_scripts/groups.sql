CREATE TABLE public.groups (
    id UUID PRIMARY KEY,       -- из BaseEntity
    objid UUID NOT NULL,
    name VARCHAR(255) NOT NULL
);

CREATE INDEX idx_groups_name ON public.groups(name);
CREATE INDEX idx_groups_objid ON public.groups(objid);
-- если нужен compound
CREATE INDEX idx_groups_objid_name ON public.groups(objid, name);
