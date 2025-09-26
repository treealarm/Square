-- \cd 'D:/TESTS/Leaflet/leaflet_dock/leaflet_data/initdb/sql_scripts'
-- \! cd
-- \i 'create_db.sql'
-- \i 'main.sql'

\connect "MapStore"
\i 'events.sql'
\i 'integro.sql'
\i 'actions.sql'
\i 'db_values.sql'
\i 'states.sql'
\i 'rights.sql'
\i 'levels.sql'
\i 'objects.sql'
\i 'properties.sql'
\i 'track_points.sql'
\i 'groups.sql'
\i 'diagram_types.sql'
\i 'diagrams.sql'