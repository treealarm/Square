using LeafletAlarmsGrpc;

namespace IntegrationUtilsLib
{
  /// <summary>
  /// Единый слой интеграции модуля-продьюсера со Square (протокол Square: gRPC поверх Dapr).
  /// Прячет транспорт (gRPC-клиенты) — продьюсер работает только через этот интерфейс,
  /// не зная про конкретные клиенты/каналы.
  ///
  /// Направление данных:
  ///  - модуль ПУШИТ своё дерево объектов, свойства, состояния, события, геометрию;
  ///  - модуль РЕГИСТРИРУЕТ объекты как интеграционные (под своим APP_ID = i_name);
  ///  - Square шлёт модулю команды (см. IObjectActions), модуль отвечает статусом (ReportActionStatus).
  /// </summary>
  public interface ISquareIntegration
  {
    /// <summary>Dapr APP_ID текущего модуля (= i_name его объектов в Square).</summary>
    string AppId { get; }

    /// <summary>Стабильный object_id по (prefix, number); кешируется.</summary>
    Task<string?> GenerateObjectId(string prefix, long number);

    // --- Запись: дерево объектов и их данные ---

    /// <summary>Создать/обновить базовые объекты дерева; возвращает актуальные объекты.</summary>
    Task<List<ProtoObject>?> UpsertObjects(ProtoObjectList objects);

    /// <summary>Пуш геометрии/местоположения (фигуры).</summary>
    Task PushFigures(ProtoFigures figures);

    /// <summary>Пуш точек трека (история перемещения).</summary>
    Task PushTracks(TrackPointsProto tracks);

    /// <summary>Пуш свойств. Всегда merge (старые свойства не удаляются).</summary>
    Task PushProperties(ProtoObjPropsList properties);

    /// <summary>Пуш состояний объектов.</summary>
    Task PushStates(ProtoObjectStates states);

    /// <summary>Пуш событий.</summary>
    Task PushEvents(EventsProto events);

    /// <summary>Пуш значений (сенсоры/переменные).</summary>
    Task PushValues(ValuesProto values);

    /// <summary>Загрузить файл (снимок/изображение и т.п.).</summary>
    Task UploadFile(UploadFileProto file);

    /// <summary>Пуш типов диаграмм.</summary>
    Task PushDiagramTypes(DiagramTypesProto diagramTypes);

    /// <summary>Пуш диаграмм.</summary>
    Task PushDiagrams(DiagramsProto diagrams);

    // --- Регистрация интеграции ---

    /// <summary>Зарегистрировать объекты как интеграционные (i_name проставляется = AppId).</summary>
    Task RegisterIntegro(IntegroListProto integros);

    /// <summary>Зарегистрировать иерархию типов интеграции (i_name проставляется = AppId).</summary>
    Task RegisterIntegroTypes(IntegroTypesProto types);

    // --- Команды ---

    /// <summary>Отчитаться о прогрессе/результате выполнения команды (по uid).</summary>
    Task ReportActionStatus(ProtoActionExeResultRequest results);

    // --- Чтение из Square ---

    /// <summary>Базовые объекты по id.</summary>
    Task<List<ProtoObject>?> GetObjects(IEnumerable<string> ids);

    /// <summary>Свойства объектов по id.</summary>
    Task<List<ProtoObjProps>?> GetProperties(IEnumerable<string> ids);

    /// <summary>Интеграционные объекты этого модуля по типу.</summary>
    Task<List<IntegroProto>?> GetIntegroByType(string type);

    /// <summary>Интеграционные объекты по id.</summary>
    Task<List<IntegroProto>?> GetIntegroByIds(IEnumerable<string> ids);
  }
}
