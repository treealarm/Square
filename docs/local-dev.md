# Локальный dev-flow: инфраструктура через mt_admin

> Square — это только приложения. Инфраструктура (Postgres/Keycloak/Valhalla/Redis/Placement) и
> провижининг realm+БД поднимаются из соседнего репозитория **mt_admin**
> (`../multitenant_admin/mt_admin`). Дублирующий стек `leaflet_dock/` удалён — больше нет двух копий
> SQL-схемы и realm-конфига, которые могли разъехаться (см. историю с `alarm_states.children_alarms`).

## Требования

- Соседний репозиторий `multitenant_admin` должен быть склонирован рядом с `Square`
  (`../multitenant_admin/mt_admin`).
- В `mt_admin` должен быть `.env`, выставляющий порты так, как их ждёт F5/`launch.json` Square:
  - `POSTGRES_PORT=5432`, `KEYCLOAK_PORT=8080`, `VALHALLA_PORT=8012`, `REDIS_PORT_IN/OUT=6379`,
    `POSTGRES_USER=keycloak`, `POSTGRES_PASSWORD=password`, `POSTGRES_DB=postgres`.
  - (если на машине `5432` уже занят сторонним postgres — переопределить публикацию порта в
    локальном override-файле compose, аналогично тому, как это было сделано в удалённом
    `leaflet_dock/docker-compose.local.yml`).

## 1. Поднять инфру из mt_admin

```bash
cd ../multitenant_admin/mt_admin
docker compose up -d postgresservice keycloakservice valhallaservice
```

Redis/Placement/Scheduler для локального F5 берутся не из compose, а из self-hosted dapr
(`dapr init`) — Square ожидает их на `localhost:6379` / `localhost:50005` соответственно.

Проверка, что всё поднялось на нужных портах:

```bash
docker ps --format '{{.Names}}: {{.Ports}}'
# postgresservice → 0.0.0.0:5432->5432
# keycloakservice → 0.0.0.0:8080->8080
# valhallaservice → 0.0.0.0:8012->8012
```

## 2. Провижинить `myrealm` (БД + keycloak realm)

Реалм Square называется `myrealm` (см. `DB_REALM_NAME` в `.vscode/launch.json`). Провижининг —
через `mt_admin.Server` (`POST /api/DB/...`, `POST /api/Provision/CreateRealm`):

- **БД** (накатывает актуальные `sql_scripts` — единственный источник истины для схемы):
  ```
  POST /api/DB/DropDB    body: "myrealm"
  POST /api/DB/CreateDB  body: "myrealm"
  ```
  ([DBProvisioningService](../../multitenant_admin/mt_admin/DbAdmin/DBProvisioningService.cs))

- **Keycloak realm** создаётся программно через `KeycloakAdmin.CreateRealmAsync` (а не импортом
  `realm-export.json`, как раньше в `leaflet_dock`) — см.
  [ProvisionController.CreateRealm](../../multitenant_admin/mt_admin/mt_admin.Server/Controllers/ProvisionController.cs).

Проверка:

```bash
# в БД должно быть 26 таблиц, включая alarm_states.children_alarms
docker exec mt_admin-postgresservice-1 psql -U keycloak -d myrealm -tAc \
  "SELECT column_name FROM information_schema.columns WHERE table_name='alarm_states' AND column_name='children_alarms';"

# realm должен отвечать
curl -s http://localhost:8080/realms/myrealm | head -c 200
```

## 3. Запустить Square

Как обычно — через F5 (`All services`) или задачу `dapr: run all apps`. Порты/realm/креды уже
совпадают с тем, что поднято в шагах 1–2, никаких дополнительных правок не требуется.

## Сквозная проверка

1. `docker compose up -d ...` (mt_admin) → postgres(5432)/keycloak(8080)/valhalla(8012) подняты.
2. Провижининг `myrealm` → 26 таблиц в БД, `alarm_states.children_alarms` есть, keycloak-realm
   `myrealm` создан.
3. Запуск Square → фронт на `localhost:8002` логинится через keycloak (`myuser`/...), карта грузится,
   машинки едут, в логах `LeafletAlarms`/`IntegrationHost` нет ошибок схемы (`column ... does not exist`).
