using Common;
using LeafletAlarmsGrpc;

namespace IntegrationUtilsLib
{
  /// <summary>
  /// Реализация <see cref="ISquareIntegration"/> поверх gRPC-клиентов Square.
  /// Транспорт (resilient-клиенты, пересоздание при разрыве) инкапсулирован здесь;
  /// продьюсеры его не видят.
  /// </summary>
  public class SquareIntegration : ISquareIntegration
  {
    /// <summary>Общий singleton для статических точек вызова (симуляторы/хост-задачи).</summary>
    public static ISquareIntegration Default { get; } = new SquareIntegration();

    public string AppId => Utils.ClientIntegro.AppId;

    public Task<string?> GenerateObjectId(string prefix, long number)
      => Utils.GenerateObjectId(prefix, number);

    // --- Запись ---

    public async Task<List<ProtoObject>?> UpsertObjects(ProtoObjectList objects)
    {
      var response = await Utils.ClientBase.Client!.UpdateObjectsAsync(objects);
      return response?.Objects.ToList();
    }

    public Task PushFigures(ProtoFigures figures)
      => Utils.ClientBase.Client!.UpdateFiguresAsync(figures).ResponseAsync;

    public Task PushTracks(TrackPointsProto tracks)
      => Utils.ClientBase.Client!.UpdateTracksAsync(tracks).ResponseAsync;

    public Task PushProperties(ProtoObjPropsList properties)
      => Utils.ClientBase.Client!.UpdatePropertiesAsync(properties).ResponseAsync;

    public Task PushStates(ProtoObjectStates states)
      => Utils.ClientBase.Client!.UpdateStatesAsync(states).ResponseAsync;

    public Task PushEvents(EventsProto events)
      => Utils.ClientBase.Client!.UpdateEventsAsync(events).ResponseAsync;

    public Task PushValues(ValuesProto values)
      => Utils.ClientBase.Client!.UpdateValuesAsync(values).ResponseAsync;

    public Task UploadFile(UploadFileProto file)
      => Utils.ClientBase.Client!.UploadFileAsync(file).ResponseAsync;

    public Task PushDiagramTypes(DiagramTypesProto diagramTypes)
      => Utils.ClientBase.Client!.UpdateDiagramTypesAsync(diagramTypes).ResponseAsync;

    public Task PushDiagrams(DiagramsProto diagrams)
      => Utils.ClientBase.Client!.UpdateDiagramsAsync(diagrams).ResponseAsync;

    // --- Регистрация интеграции (i_name = AppId проставляется здесь) ---

    public Task RegisterIntegro(IntegroListProto integros)
    {
      foreach (var integro in integros.Objects)
      {
        integro.IName = AppId;
      }
      return Utils.ClientIntegro.Client!.UpdateIntegroAsync(integros).ResponseAsync;
    }

    public Task RegisterIntegroTypes(IntegroTypesProto types)
    {
      foreach (var type in types.Types_)
      {
        type.IName = AppId;
      }
      return Utils.ClientIntegro.Client!.UpdateIntegroTypesAsync(types).ResponseAsync;
    }

    // --- Команды ---

    public Task ReportActionStatus(ProtoActionExeResultRequest results)
      => Utils.ClientIntegro.Client!.UpdateActionResultsAsync(results).ResponseAsync;

    // --- Чтение ---

    public async Task<List<ProtoObject>?> GetObjects(IEnumerable<string> ids)
    {
      var request = new ProtoObjectIds();
      request.Ids.AddRange(ids);
      var response = await Utils.ClientBase.Client!.RequestObjectsAsync(request);
      return response?.Objects.ToList();
    }

    public async Task<List<ProtoObjProps>?> GetProperties(IEnumerable<string> ids)
    {
      var request = new ProtoObjectIds();
      request.Ids.AddRange(ids);
      var response = await Utils.ClientBase.Client!.RequestPropertiesAsync(request);
      return response?.Objects.ToList();
    }

    public async Task<List<IntegroProto>?> GetIntegroByType(string type)
    {
      var request = new GetListByTypeRequest()
      {
        IName = AppId,
        IType = type
      };

      try
      {
        var response = await Utils.ClientIntegro.Client!.GetListByTypeAsync(request);
        return response?.Objects.ToList();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return null;
      }
    }

    public async Task<List<IntegroProto>?> GetIntegroByIds(IEnumerable<string> ids)
    {
      var request = new ProtoObjectIds();
      request.Ids.AddRange(ids);
      var response = await Utils.ClientIntegro.Client!.GetListByIdsAsync(request);
      return response?.Objects.ToList();
    }
  }
}
