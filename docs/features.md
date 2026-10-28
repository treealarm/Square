# Square — ключевые фичи и как ими пользоваться

> Краткий обзор возможностей системы. Как поднять окружение локально — см.
> [local-dev.md](local-dev.md).

## Суть проекта

Square — платформа мониторинга объектов в реальном времени. Объекты (машины, камеры, датчики,
здания, зоны…) живут в **дереве**, могут иметь **геометрию на карте** (Leaflet), **состояния и
тревоги**, **события**, **живые значения** (телеметрию), **треки** перемещения и **мнемосхемы**
(диаграммы). Данные поставляют внешние **интеграции** по gRPC; они же исполняют **действия
(команды)**, которые оператор запускает из UI.

```
 Интеграции (камеры, генератор треков, ...)
        │ gRPC через Dapr (app-id: integrationhost)
        ▼
 IntegrationHost ──► PostgreSQL ◄── LeafletAlarms (REST + WebSocket /state + SPA)
        │                                  ▲
        └─ pub/sub (Redis) ─► BlinkService │ (поллинг БД раз в 1 с)
                                           │
                                  tree_alarm.client (React + MUI + Leaflet)
```

| Компонент | Роль |
|---|---|
| [LeafletAlarms](../LeafletAlarms) | REST API, WebSocket `/state`, раздача React-клиента, тайлы карты |
| [IntegrationHost](../IntegrationHost) | Приём данных от интеграций по gRPC, запись в ту же БД |
| [BlinkService](../BlinkService) | Распространение тревог вверх по дереву (Dapr Actors) |
| [AASubService](../AASubService) | Интеграция ONVIF-камер |
| [GrpcTracksClient](../GrpcTracksClient) | Демо-продьюсер: машины по Москве, тревоги, события, диаграммы |
| [tree_alarm.client](../tree_alarm.client) | Веб-интерфейс оператора |

Инфраструктура (Postgres/PostGIS, Keycloak, Valhalla, Redis, Dapr placement) поднимается из
соседнего репозитория `mt_admin`, а не из этого `docker-compose.yml`.

---

## 1. Основные понятия

### Объект и дерево
Любая сущность — объект `{id, parent_id, owner_id, name}`.
- `parent_id` строит иерархию. Удаление объекта каскадно удаляет всех потомков.
- `owner_id` делает объект **репликой (view)** другого объекта: реплика своих состояний и значений
  не хранит, а показывает состояния/тревоги/значения владельца. Так один датчик можно разместить в
  нескольких местах дерева, на карте и на разных диаграммах.
- ID — 24-символьная строка в стиле ObjectId. Интеграции генерируют детерминированные ID через
  `GenerateObjectId(input, version)`.

### Свойства (extra_props)
Произвольные пары `prop_name / str_val / visual_type`. `visual_type` подсказывает UI, как
отобразить/редактировать значение: `__clr` (цвет), `__pass` (пароль), `__snapshot` (снимок
камеры), `__ip_range`, `__credential_list`, `__enum_list`, `__coordinates`, `__map` и т.д.
([VisualTypes.cs](../Domain/VisualTypes.cs)).

Служебные свойства влияют на вид маркера: `__color`, `__image`, `__image_width/height`,
`__image_rotate`, `__text`. По свойствам можно искать объекты.

### Геометрия
Три типа фигур: `Point` (с необязательным `radius` — тогда это круг), `Polygon`, `LineString`.
Координаты в порядке Leaflet `[lat, lon]`. `zoom_level` ограничивает диапазон масштабов, на которых
объект виден.

### Состояния и тревоги
- Объекту присваивается набор состояний-строк (`NORM`, `ALARM`, `CONNECTED`, `DISCONNECTED`,
  `INFO` по умолчанию). Справочник состояний задаёт описание, цвет и флаг `alarm`.
- Если у объекта есть состояние с `alarm=true` — объект в тревоге (красный).
- BlinkService считает `children_alarms`: **родитель подсвечивается красным, если тревога у любого
  потомка**. Тревогу видно с верхних уровней дерева.

### События
`{timestamp, object_id, event_name, event_priority, param0, param1, extra_props}`. Могут нести
картинки: интеграция передаёт свойство `base64image_fs`, сервер сохраняет файл и превращает его в
ссылку `image_fs`.

### Значения (values)
Живые именованные значения объекта (`speed`, `temperature`, …), тип — double/int/string.
Отображаются на диаграммах.

### Треки
История положений объекта во времени (точка/линия/полигон + свойства). Пишутся отдельно или
автоматически при `UpdateFigures` с `add_tracks=true`.

### Диаграммы (мнемосхемы)
- **Тип диаграммы** — картинка + именованные **регионы** (в долях 0..1).
- **Диаграмма** привязана к объекту (id = id объекта): тип, геометрия
  (left/top/width/height/rotation), фон, необязательный `region_id` — регион в типе
  **родителя**, куда она вписывается.
- Дочерние объекты с диаграммами рисуются вложенными, так собирается схема любой глубины.
- Если имя значения объекта совпадает с `region_key` региона, в этом регионе показывается
  `name: value`.

### Интеграционные объекты и действия
- Объект может принадлежать интеграции: `{i_name = Dapr app-id продьюсера, i_type}`.
- Интеграция регистрирует **типы** и допустимые дочерние типы (например `main → cam`). UI по ним
  предлагает «Add new typed child».
- **Действия** (actions) объявляет и исполняет интеграция: discover, PTZ, get_snapshot и т.п.
  Прогресс исполнения возвращается в Square и виден в UI.

### Права
Для каждого объекта задаются строки `{role, value}`, где `role` — роль Keycloak или имя
пользователя, а `value` — `View` и/или `Control`. **Объект без строк прав виден всем.**
Роли Keycloak: `admin`, `power_user`, `user`.

---

## 2. Веб-интерфейс: как пользоваться

### Вход
`/login` — логин/пароль, бэкенд сам ходит в Keycloak. Пока бэкенд не отвечает, показывается
«Connecting to server...». Токен обновляется автоматически.

### Экраны

| URL | Что это |
|---|---|
| `/` | Главный экран: дерево, карта/диаграмма, панели свойств |
| `/?selectedId=<id>` | Открыть главный экран с выбранным объектом |
| `/_states` | Таблица состояний и тревог |
| `/_events` | Журнал событий (таблица / галерея картинок) |
| `/_monitor` | Камеры и мониторы (vms_rec) |
| `/_editdiagrams` | Редактор диаграмм |
| `/editdiagram` | Редактор типов диаграмм |

### Главный экран (`/`)

**Тулбар** (слева направо): левая панель (дерево / результаты поиска), индикатор WebSocket
(клик — ping), «моё местоположение», навигация по диаграммам, переходы на States / Events /
Monitor / Diagram Editor, чекбоксы слоёв **Objects / Tracks / Update Values**, вход/выход, правая
панель (Search / Properties / Track props / Rights / Actions). Раскладка панелей запоминается в
браузере.

**Дерево объектов**
- Хлебные крошки (ROOT → …) — переход вверх; шеврон у строки — вход в детей.
- Клик по строке — выбор объекта (повторный клик снимает выбор). Чекбокс отмечает объекты для
  States Viewer.
- На выбранной строке: **Add new child**, **Add new typed child** (для интеграционных типов),
  **Import from file** (JSON-массив `{obj, integro}`).
- Строку можно **перетащить на карту**: объект получит точку в месте броска (полигоны/линии
  не перезаписываются). Также строку можно перетащить на диаграмму.

**Карта**
- Подгружает объекты видимой области. Цвет маркера: тревога у объекта или потомка → красный,
  иначе цвет первого состояния, иначе `__color`.
- Когда в области больше ~1000 фигур, сервер отдаёт верхние 500 и **кластеры** (KMeans) вместо
  остальных.
- Ctrl+клик копирует координаты в буфер обмена.

**Создание и редактирование фигур** (панель Properties)
1. Выберите родительский объект → **Create new object** → Circle / Polyline / Polygon.
   Включится режим редактирования, фигура подсвечена жёлтым.
2. Клик по карте ставит центр круга или добавляет вершину. В попапе вершины доступны
   Delete / Move / Insert / Add vertex, **Move Polygon** (перенос всей фигуры) и Save.
3. **Save** в панели сохраняет свойства и геометрию; **Edit** включает и выключает режим
   правки; Esc отменяет перемещение.
4. Без карты: **Add Geo** ставит точку в центр карты, координаты вершин, radius, zoom_level и
   rotation правятся полями.

**Панель Properties** также умеет:
- менять Name, ParentId, OwnerId (с пикером объекта);
- **Find object on map**;
- **Build route** для полилинии (маршрут через Valhalla);
- добавлять, удалять и редактировать свойства;
- для `__snapshot` открывать окно с обновляемым снимком камеры и PTZ;
- **создавать реплику**: перетащить иконку-копию на карту или диаграмму. Появится новый объект с
  `owner_id` = текущий и копией свойств.

**Поиск и треки** (панель Search)
1. Задайте **begin/end** и при желании фильтр «свойство = значение».
2. Включите **Apply**: фильтр применится к объектам на карте и к трекам за период. Стрелки рядом
   с датами переключают на предыдущий/следующий трек.
3. Слева в **Search result** — точки треков по времени. Клик выбирает трек (красный пунктир),
   детали в **Track props**.

### Диаграммы

**Просмотр:** выберите объект с диаграммой → **Dive** в тулбаре, карта сменится схемой.
- **Depth** (0–3) — сколько вложенных уровней показывать.
- Клик выбирает элемент, повторный Dive — вход в него. Вкладки родителей ведут вверх,
  **Resurface** возвращает на карту.
- Рамки элементов окрашены по состоянию, красные при тревоге. Значения в регионах обновляются
  вживую, если включён **Update Values**.

**Редактирование** (`/_editdiagrams`; изменять диаграммы можно только здесь, нужна роль
admin/power_user):
1. Слева — дерево с поиском по имени. 📎 прикрепляет реплику найденного объекта к открытой
   диаграмме.
2. Выберите объект → справа **Add Diagram for Object**.
3. Задайте тип (кнопка select открывает редактор типов, Ok — назначить), регион родителя, фон
   (ссылка или загрузка файла), положение, размер и поворот.
4. На холсте: перетаскивайте строки дерева/реплики, двигайте свободные элементы (без
   `region_id`), поворачивайте ручкой. Затем **Save**.
5. **Diagram composition** показывает детей диаграммы; их можно перетащить на холст.

**Типы диаграмм** (`/editdiagram`): поиск типов; для типа задаются Name, картинка Src (можно
загрузить) и список регионов (id + left/top/width/height в долях).

### Состояния (`/_states`)
Отметьте объекты чекбоксами в дереве слева — в таблице появятся их состояния (цветные чипы) и
тревоги 🚨, обновляемые в реальном времени. Клик по строке показывает детали состояний и снимок
камеры, если он есть.

### События (`/_events`)
- Фильтры: период, группы объектов, Param 0 / Param 1, текстовый поиск по имени события,
  **Gallery view** (только события с картинками).
- Кнопка первой страницы одновременно включает **автообновление** (каждые 3 с).
- Таблица сортируется по нескольким колонкам. Клик по событию показывает поля и картинки.

### Действия (панель Actions)
1. Выберите интеграционный объект → откройте **Actions**. Появится список доступных действий от
   интеграции.
2. Заполните параметры: под тип есть редакторы — диапазон IP, список логинов/паролей, enum,
   координаты, словарь, числа.
3. **Execute**. Сверху отображаются текущие исполнения с прогресс-баром и **Cancel**.

Пример: на корневом объекте AASubService действие `discover` с диапазоном IP и учётками сканирует
сеть и само создаёт объекты-камеры.

### Права (панель Rights, только admin)
Для выбранного объекта добавьте строку, введите роль или пользователя (есть подсказки ролей),
отметьте View/Control и нажмите **Save**.

### Монитор (`/_monitor`)
Работает поверх внешнего сервиса **vms_rec** (`VITE_VMS_REC_BASE_URL`, тот же JWT).
- Слева: мониторы (раскладки 1x1…3x3) и быстрый просмотр камер (объекты `vmscfg/camera`).
- В карточке камеры: live (WebRTC/WHEP) / архив (HLS, список записей), PTZ для поддерживающих
  ONVIF-камер, Arm/Disarm, непрерывная запись, профиль потока.
- `/_monitor?cameraId=<id>` открывает одну камеру.

---

## 3. Интеграции: как подключить свой источник данных

Интеграция — отдельный процесс со своим Dapr-сайдкаром и `APP_ID`. Она ходит в Square по gRPC
через Dapr service invocation на app-id **`integrationhost`**. Контракт описан в
[Protos/](../Protos) (источник истины — сабмодуль
[SquareIntegrationClient](../SquareIntegrationClient)).

**Что можно отправить** (`tracks.TreeAlarmsGrpcService`, `integro.IntegroService`):

| Метод | Назначение |
|---|---|
| `UpdateObjects` / `RequestObjects` | Создать/обновить/прочитать объекты дерева |
| `UpdateFigures` | Геометрия + свойства (опционально сразу трек) |
| `UpdateProperties` / `RequestProperties` | Свойства (слияние или замена `replace_props`) |
| `UpdateStates` | Состояния (`ALARM`, `NORM`, …) |
| `UpdateTracks` | Точки треков |
| `UpdateEvents` | События, в т.ч. с картинками |
| `UpdateValues` | Живые значения |
| `UpdateDiagramTypes` / `UpdateDiagrams` | Мнемосхемы |
| `UploadFile` | Файл в общее хранилище (`/leaflet_data`) |
| `GenerateObjectId` | Детерминированный ID из (строка, версия) |
| `UpdateIntegro` / `UpdateIntegroTypes` | Привязать объекты к интеграции, объявить типы |
| `UpdateActionResults` | Прогресс/результат исполнения действия |

**Что интеграция реализует сама** (`object_actions.ActionsService`; Square вызывает его по app-id
из `i_name` объекта): `GetAvailableActions`, `ExecuteActions`, `CancelActions`,
`ExecuteActionGetResult` (синхронный вызов, например `get_snapshot` возвращает картинку).

**Быстрый старт на C#** (образец — [AASubService](../AASubService)):
1. Взять `ISquareIntegration` из [SquareIntegrationClient](../SquareIntegrationClient) — фасад
   `UpsertObjects`, `PushFigures`, `PushStates`, `PushEvents`, `PushValues`, `RegisterIntegro`,
   `ReportActionStatus`, … (`i_name` проставляется автоматически). В
   [IntegrationUtilsLib](../IntegrationUtilsLib) есть готовый `SquareIntegration.Default`, он
   берёт `APP_ID` и `LEAFLETALARM_APP_ID` (app-id приёмника, исторически названный так) из env.
2. Использовать `IntegrationSyncFull` ([IntegrationUtilsLib](../IntegrationUtilsLib)). Он создаёт
   корневой объект интеграции (`i_type = "main"`), регистрирует дерево типов, достраивает
   недостающие свойства по шаблонам и подписывается на `OnUpdateIntegros_<AppId>` /
   `OnDeleteIntegros_<AppId>`. Правки объектов интеграции в UI прилетают в коллбэк
   `OnConfigurationChanged`.
3. Реализовать `IObjectActions` и поднять `ActionsServiceImpl` на HTTP/2-порту.
4. Запустить Dapr-сайдкар с `--app-protocol grpc --app-port <port>`.

Как работает действие от начала до конца: UI → `api/Integro/ExecuteActions` → Square сохраняет
исполнение с `uid` → вызывает `ExecuteActions` у интеграции → интеграция шлёт
`UpdateActionResults(uid, progress, result)` → прогресс виден в панели Actions.

### Готовые интеграции

- **AASubService** — ONVIF-камеры:
  - автодискавери по диапазону IP;
  - статус CONNECTED/DISCONNECTED каждые 3 с;
  - снимок каждые 30 с (свойство `__snapshot`);
  - ONVIF-события превращаются в события Square;
  - действия `telemetry` (PTZ), `refresh`, `get_snapshot`.
- **GrpcTracksClient** — демо-данные:
  - 50 машин ездят по маршрутам Valhalla вокруг Москвы, у них есть значения `speed` и
    `temperature`;
  - точка, движущаяся по Садовому кольцу;
  - объекты, мигающие ALARM/NORM;
  - события с номерами машин и картинками;
  - диаграммы машин;
  - действия `Occupate/Free`, `SetCarParams` (новая точка назначения — машина перестраивает
    маршрут).

---

## 4. Запуск и развёртывание (кратко)

- **Локально (F5):** инфра из `mt_admin`, сервисы — конфигурация «All services» в
  [.vscode/launch.json](../.vscode/launch.json). Подробно — [local-dev.md](local-dev.md).
  Альтернатива — `dapr run -f dapr.yaml`.
- **Docker:** [build-and-run.sh](../build-and-run.sh) (`docker compose build && up`). У каждого
  сервиса есть свой `daprd`-сайдкар. Порты: LeafletAlarms `8000`, IntegrationHost `5000` (gRPC),
  GrpcTracksClient `20007`, AASubService `20008`.
- **Публикация образов:** [push-images.sh](../push-images.sh) → `treealarm/<name>:latest`.
- **Обязательные env бэкенда:** `KEYCLOAK_URL`, `KEYCLOAK_ADMIN_USER`, `KEYCLOAK_ADMIN_PASSWORD`,
  `KEYCLOAK_CLIENT_ID`, `DB_REALM_NAME` (имя realm и БД одновременно), `POSTGRES_USER`,
  `POSTGRES_PASSWORD`, `PUBSUB_NAME`, `VALHALLA_URL`.
- **Swagger:** `/swagger` (только в Development).

## 5. Для разработчика: где что лежит

| Тема | Где смотреть |
|---|---|
| REST API | [LeafletAlarms/Controllers](../LeafletAlarms/Controllers), все пути `api/<Name>/...` |
| Живые обновления | [StateWebSocket.cs](../LeafletAlarms/Services/StateWebSocket.cs): сервер раз в 1 с поллит БД и шлёт клиенту только ID изменений, детали клиент добирает через REST |
| Авторизация | [LeafletAlarms/Authentication](../LeafletAlarms/Authentication): JWT Keycloak, по умолчанию всё требует аутентификации |
| Проверка прав | [RightsCheckerService.cs](../LeafletAlarms/Services/RightsCheckerService.cs) |
| Запись данных | [DataChangeLayer](../DataChangeLayer) |
| Схема БД / запросы | [DbLayer/pg_services](../DbLayer/pg_services) |
| Приём gRPC | [IntegrationHost/GRPCServiceProxy.cs](../IntegrationHost/GRPCServiceProxy.cs) |
| Тревоги по иерархии | [BlinkService/HierarhyStateService.cs](../BlinkService/HierarhyStateService.cs) |
| Pub/sub-топики | [Topics.cs](../Domain/PubSubTopics/Topics.cs): `AlarmStatesChanged`, `OnUpdateIntegros_<app>`, `OnDeleteIntegros_<app>` |
