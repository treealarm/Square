-- \cd 'D:/TESTS/Leaflet/leaflet_dock/leaflet_data/initdb/sql_scripts'
-- \! cd
-- \i 'create_db.sql'
-- \i 'main.sql'

\connect "MapStore"
\i 'events.sql'
\i 'integro.sql'
\i 'actions.sql'