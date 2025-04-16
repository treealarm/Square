-- \cd 'D:/TESTS/Leaflet/DbLayer/sql'
-- \! cd
-- \i 'create_db.sql'
-- \i 'main.sql'

\connect "MapStore"
\i 'sql_scripts/events.sql'
\i 'sql_scripts/integro.sql'


