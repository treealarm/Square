using IntegrationUtilsLib;

namespace AASubService
{
  public interface ICameraManager: IAsyncDisposable, IObjectActions
  {
    Task DoWork(CancellationTokenSource cancellationToken);
  }
}
