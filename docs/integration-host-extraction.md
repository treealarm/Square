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

**Почему это возможно без поломок:** write-путь уже развязан с фронтом через шину —
`TracksUpdateService`/`StatesUpdateService` пишут в БД и публикуют в pub-sub (`IPubService`),
а фронтовый [WebSockListService](../LeafletAlarms/Services/WebSockListService.cs) **подписан**
на pub-sub (`ISubService`). Значит, запись из отдельного процесса → БД + pub-sub → WebSocket
LeafletAlarms подхватит. Живые обновления сохраняются.

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
  LeafletAlarms подписан. Уже работает кросс-процессно.
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
2. **Новый проект** `IntegrationHost` (ASP.NET, Kestrel HTTP/2): референс Domain, DataChangeLayer,
   DbLayer, протоколы, `IntegrationContracts`. Перенести папку `Grpc/`.
3. **DI в IntegrationHost** `Program.cs`: `AddGrpc` + интерсептор, `MapGrpcService<TracksGrpcImp>` и
   `<IntegroGrpcImp>`, регистрация DbContext (без миграций), DataChangeLayer write-сервисов,
   `IPubService`, `FileSystemService`. Переиспользовать существующее DI-расширение DbLayer/DataChangeLayer
   (найти его в LeafletAlarms/Startup при реализации).
4. **LeafletAlarms**: убрать `MapGrpcService<TracksGrpcImp/IntegroGrpcImp>` и хостинг приёмного gRPC;
   оставить REST/WebSocket/northbound. (gRPC `AddGrpc` оставить только если ещё что-то хостит.)
5. **docker-compose / .env / launch.json**: сервис `integrationhost` + сайдкар; общий volume static-files
   в оба; те же DB/Redis env; продьюсерам `LEAFLETALARM_APP_ID=integrationhost`.
6. **Миграции**: убедиться, что только LeafletAlarms их применяет.

## Проверка

- **Автономный фронт:** LeafletAlarms стартует без приёмного gRPC; REST/WebSocket/React работают.
- **Продьюсеры → IntegrationHost:** GrpcTracksClient двигает машинки, AASubService шлёт снапшоты/
  состояния → данные в БД, события в pub-sub.
- **Живой фронт:** изменения от продьюсеров видны в UI через WebSocket (через pub-sub) — т.е.
  кросс-процессный путь работает.
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
