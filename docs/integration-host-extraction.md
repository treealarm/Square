# Вынос приёмного gRPC-слоя интеграции в отдельный процесс (IntegrationHost)

> Статус: **план, не реализован.** Решение по запуску — отдельной задачей.

## Context / зачем

Сейчас приёмная gRPC-часть (`TracksGrpcImp`, `IntegroGrpcImp`, `GRPCServiceProxy`) хостится
**внутри `LeafletAlarms`** ([Program.cs](../LeafletAlarms/Program.cs) `MapGrpcService<...>`),
вместе с REST/WebSocket/раздачей React. Цель — вынести её в самостоятельный процесс
**IntegrationHost**, чтобы:
- продьюсеры (GrpcTracksClient, AASubService, в будущем vms_rec) долбили **только** этот процесс,
  не влияя на фронт-API;
- интеграционный слой деплоился/перезапускался/масштабировался независимо.

**Почему это возможно без поломок:** фронт-обновления развязаны с write-путём через **общую БД**,
а не через pub-sub. Фронтовый [StateWebSocket](../LeafletAlarms/Services/StateWebSocket.cs) в цикле
`DoWork` **поллит БД раз в секунду** (`PollBox`/`PollStates`/`PollValues`). Значит запись из отдельного
процесса (IntegrationHost) в ту же PostgreSQL подхватится поллингом фронта. Живые обновления
сохраняются и вообще не зависят от pub-sub.
(Pub-sub — `IPubService` в `TracksUpdateService`/`StatesUpdateService` — используется ДРУГИМИ
подписчиками: `BlinkService` (`AlarmStatesChanged`) и продьюсерским `IntegrationSyncFull`
(`OnUpdateIntegros`), не фронтовым сокетом.)

**Важно:** это НЕ «тонкий слой». `GRPCServiceProxy` зависит от write-сервисов
(`ITracksUpdateService`, `IMapService`, `FileSystemService`, …), поэтому IntegrationHost тащит
**DataChangeLayer + DbLayer** и коннектится к той же PostgreSQL. Это второй backend-процесс,
делящий с LeafletAlarms БД, Redis и хранилище файлов.

## Что переезжает / что остаётся

**В IntegrationHost (новый ASP.NET gRPC-хост):**
- [Grpc/](../LeafletAlarms/Grpc/): `TracksGrpcImp`, `IntegroGrpcImp`, `GRPCServiceProxy`,
  `GrpcContextInterceptor`.
- Хостинг `MapGrpcService<TracksGrpcImp>` / `MapGrpcService<IntegroGrpcImp>` + `AddGrpc` с интерсептором.
- DI write-слоя: DataChangeLayer-сервисы (`ITracksUpdateService`, `IStatesUpdateService`,
  `IEventsUpdateService`, `IValuesUpdateService`, `IDiagramTypeUpdateService`,
  `IDiagramUpdateService`, `IIntegroUpdateService`, `IMapService`), DbLayer (DbContext), `IPubService`.

**Остаётся в LeafletAlarms:**
- REST-контроллеры (`MapController`, `FilesController`, `IntegroController` northbound),
  WebSocket (`WebSockListService`/`StateWebSocket`), auth, раздача React.
- Форвард команд продьюсерам из `IntegroController` (Dapr invocation на app_id продьюсера) — без изменений.
- Свой DI read/write-сервисов для REST.

## Связки, которые надо распутать (вынести в общую библиотеку)

Общий серверный lib **`IntegrationServerLib`** (симметрично продьюсерскому `IntegrationUtilsLib`),
референсит Domain + GrpcDaprLib, подключён к обоим процессам:
- **`FileSystemService`** — используется и `FilesController` (остаётся), и `GRPCServiceProxy`
  (уезжает). **[СДЕЛАНО — Step 1]** перенесён в `IntegrationServerLib`.
- **Геоконвертеры `GRPCServiceProxy.CoordsFromProto2DTO` / `ConvertGeoDTO2Proto`** — статика,
  используется `ProtoToDTOConvertor` → `IntegroController` (остаётся). **[СДЕЛАНО — Step 1]**
  вынесены в `IntegrationServerLib.GeometryProtoConvert`; `GRPCServiceProxy` и `ProtoToDTOConvertor`
  зовут его оттуда.
- **`GrpcRequestContextProvider`** (Authentication) — нужен интерсептору в IntegrationHost.
  **Отложено (YAGNI):** в LeafletAlarms REST-путь использует `HttpRequestContextProvider`, а
  `GrpcRequestContextProvider` нужен только gRPC-приёму → переедет вместе с IntegrationHost (Step 2),
  в общий lib сейчас не выносим.

> Примечание: proto-типы `LeafletAlarmsGrpc` генерятся в **GrpcDaprLib** и попадают в LeafletAlarms
> транзитивно через ValhallaLib. IntegrationHost должен ссылаться на GrpcDaprLib напрямую.

## Общая инфраструктура

- **PostgreSQL:** оба процесса через EF. Миграции/`EnsureCreated` владеет **только LeafletAlarms**;
  IntegrationHost лишь подключается (не мигрирует) — иначе гонка схемы.
- **Redis pub-sub (Dapr):** IntegrationHost публикует `OnUpdateIntegros`/`AlarmStatesChanged`;
  подписчики — `BlinkService` и продьюсерский `IntegrationSyncFull` (НЕ фронт LeafletAlarms).
  Уже работает кросс-процессно.
- **Static-files volume:** `FileSystemService.Upload` пишет в локальную папку (base path из
  `RoutingSettings`), а `FilesController` раздаёт. Нужен **общий volume**, смонтированный в оба
  контейнера по одному пути (иначе снимки камер не отобразятся). Путь — через общий env/настройку.

## Dapr / app_id / env

- Новый Dapr app_id, напр. **`integrationhost`**, + свой сайдкар/контейнер.
- Продьюсеры: `LEAFLETALARM_APP_ID` → **`integrationhost`** (они зовут `TracksGrpcImp`/`IntegroGrpcImp`,
  которые теперь там). Менять не код ([GrpcBaseUpdater](../GrpcDaprLib/GrpcBaseUpdater.cs) читает env),
  а env в compose/launch.json.
- `IntegroController` → продьюсеры: без изменений (зовёт app_id продьюсеров).
- Проверено: in-proc вызывателей `GRPCServiceProxy` в LeafletAlarms нет (REST дёргает update-сервисы
  напрямую, не прокси) — переезд чистый.

## Шаги

1. **[СДЕЛАНО] Общий lib** `IntegrationServerLib`: перенесён `FileSystemService`, выделен
   `GeometryProtoConvert` (из статики `GRPCServiceProxy`). Поправлены ссылки в `FilesController`,
   `Startup`, `GRPCServiceProxy`, `ProtoToDTOConvertor`. LeafletAlarms собирается.
   (`GrpcRequestContextProvider` отложен — переедет с IntegrationHost.)
2. **[СДЕЛАНО] Новый проект** `IntegrationHost` (ASP.NET, Kestrel HTTP/2): референс Domain,
   DataChangeLayer, DbLayer, GrpcDaprLib, `IntegrationServerLib`, PubSubLib. Перенесены (git mv,
   namespace → `IntegrationHost`): `TracksGrpcImp`, `IntegroGrpcImp`, `GRPCServiceProxy`,
   `GrpcContextInterceptor`, `GrpcRequestContextProvider`. (`ProtoToDTOConvertor` остался в LeafletAlarms
   — нужен `IntegroController`.) Доп. находка: `GenerateObjectId(input,version)` тоже общий → вынесен
   в `IntegrationServerLib.ObjectIdGenerator`.
3. **[СДЕЛАНО] DI в IntegrationHost** `Program.cs`: `AddGrpc`+интерсептор, `MapGrpcService<...>`,
   `DbLayer.ServicesConfigurator` + `DataChangeLayer.ServicesConfigurator` (переиспользованы),
   pub-sub, `FileSystemService`, `GRPCServiceProxy`. Миграции не запускаем.
   Realm/auth НЕ переносили: БД выбирается `DB_REALM_NAME`, а `IRequestContextProvider` в DbLayer
   опционален (`GetService` → null) и его результат не используется.
4. **[СДЕЛАНО] LeafletAlarms**: убраны `MapGrpcService`/`AddGrpc`/gRPC-listener, регистрации
   `GRPCServiceProxy` и `GrpcRequestContextProvider`. Остался REST/WebSocket/SPA/northbound.
5. **[СДЕЛАНО] docker-compose / dapr.yaml / .env**: сервис `integrationhostservice` + сайдкар
   (`integrationhost`, grpc, `GRPC_INTEGRATION_PORT`); общий том `${ROOT_DATA}:/leaflet_data` в оба;
   продьюсерам (compose+dapr.yaml) `LEAFLETALARM_APP_ID=integrationhost`; сайдкар leafletalarms →
   `--app-protocol http --app-port ${HTTP_PORT}`. Новые `IntegrationHost/Dockerfile` и
   `Properties/launchSettings.json`. `docker compose config` валиден, solution собирается (0 ошибок).
6. **Миграции**: IntegrationHost не регистрирует `InitHostedService` → их применяет только LeafletAlarms. ✓

> **Не проверено в рантайме (нужен полный стек Dapr+Postgres+Redis+Keycloak):** фактический приём
> пушей integrationhost'ом, фронт-поллинг общей БД (StateWebSocket), общий том для снимков,
> корректность смены протокола сайдкара leafletalarms (http). См. раздел «Проверка».

## Проверка

- **Автономный фронт:** LeafletAlarms стартует без приёмного gRPC; REST/WebSocket/React работают.
- **Продьюсеры → IntegrationHost:** GrpcTracksClient двигает машинки, AASubService шлёт снапшоты/
  состояния → данные в БД, события в pub-sub.
- **Живой фронт:** изменения от продьюсеров видны в UI — `StateWebSocket` поллит общую БД, куда
  пишет IntegrationHost (кросс-процессный путь через общую БД, без pub-sub).
- **Файлы:** снапшот, загруженный IntegrationHost, отдаётся `FilesController` LeafletAlarms (общий volume).
- **Команды:** `get_snapshot`/PTZ/`Occupate` из UI (LeafletAlarms `IntegroController`) доходят до
  продьюсера, статус по uid возвращается (`UpdateActionResults` → IntegroGrpcImp в IntegrationHost).
- **Изоляция:** нагрузка на IntegrationHost (поток пушей) не роняет фронт-API LeafletAlarms.

## Риски / открытые вопросы

- Два EF-клиента к одной БД: пулы соединений, кто владеет миграциями (решено: LeafletAlarms).
- Общий volume static-files обязателен; иначе битые ссылки на изображения.
- `GrpcRequestContextProvider`/`IRequestContextProvider` — проверить всех потребителей перед переносом.
- Доп. контейнер + сайдкар + env во всех обязательных местах (compose, .env, dockerhub/.env, launch.json).
- YAGNI: делать только при реальной потребности в изоляции/независимом деплое; граница интеграции
  уже обеспечена контрактом + фасадом `ISquareIntegration` даже без разделения процессов.
