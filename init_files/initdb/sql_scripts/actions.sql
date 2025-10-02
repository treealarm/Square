CREATE TABLE IF NOT EXISTS public.action_executions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  object_id UUID NOT NULL,
  name TEXT NOT NULL,
  "timestamp" timestamp with time zone
);

CREATE TABLE IF NOT EXISTS public.action_parameters (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  action_execution_id UUID NOT NULL REFERENCES action_executions(id) ON DELETE CASCADE,
  name TEXT NOT NULL,
  type TEXT NOT NULL,
  cur_val JSONB
);

CREATE TABLE IF NOT EXISTS public.action_result (
  id UUID PRIMARY KEY REFERENCES action_executions(id) ON DELETE CASCADE,
  progress INTEGER,
  result JSONB
);

