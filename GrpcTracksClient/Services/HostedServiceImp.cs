using IntegrationUtilsLib;

namespace GrpcTracksClient.Services
{
  public class HostedServiceImp : IHostedService
  {
    private readonly IMoveObjectService _moveObjectService;
    private List<Task> _tasks = new List<Task>(); // Для хранения ссылок на задачи

    public HostedServiceImp(IMoveObjectService moveObjectService)
    {
      _moveObjectService = moveObjectService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await Task.Delay(1);
      // Инициализируем задачи
      _tasks.Add(Utils.RunTaskWithRetry(() => _moveObjectService.MoveCars(cancellationToken), "MoveCars", cancellationToken));
      _tasks.Add(Utils.RunTaskWithRetry(() => _moveObjectService.MovePolygons(cancellationToken), "MovePolygons", cancellationToken));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      // Ожидаем завершения всех задач с учетом отмены
      var cancellationTask = Task.WhenAny(_tasks);

      // Если отмена запроса произошла, отменяем все задачи
      if (cancellationTask == Task.WhenAny(_tasks))
      {
        cancellationToken.ThrowIfCancellationRequested();
      }

      await Task.WhenAll(_tasks); // Ожидаем завершения всех задач
    }
  }
}
