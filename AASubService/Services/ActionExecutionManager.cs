using ObjectActions;
using System.Collections.Concurrent;

namespace AASubService
{
  public class ActionExecutionManager
  {
    private readonly ConcurrentDictionary<string, (ProtoActionExe Action, CancellationTokenSource Cts)> _runningTasks = new();

    private readonly object _lock = new();

    public bool IsActionWithNameRunning(string name)
    {
      return _runningTasks.Values.Any(v => v.Action.Name == name);
    }


    public bool TryStartAction(ProtoActionExe action, Func<ProtoActionExe, CancellationToken, Task> actionHandler)
    {
      lock (_lock)
      {
        if (_runningTasks.ContainsKey(action.Uid))
          return false;

        var cts = new CancellationTokenSource();
        _runningTasks[action.Uid] = (action, cts);

        _ = Task.Run(async () =>
        {
          try
          {
            await actionHandler(action, cts.Token);
          }
          catch (OperationCanceledException)
          {
            // Логика при отмене
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex);
          }
          finally
          {
            _runningTasks.TryRemove(action.Uid, out _);
          }
        }, cts.Token);

        return true;
      }
    }

    public bool CancelAction(string uid)
    {
      if (_runningTasks.TryRemove(uid, out var cts))
      {
        cts.Cts.Cancel();
        return true;
      }

      return false;
    }

    public void CancelAll()
    {
      foreach (var cts in _runningTasks.Values)
      {
        cts.Cts.Cancel();
      }

      _runningTasks.Clear();
    }

    public bool IsRunning(string uid) => _runningTasks.ContainsKey(uid);
  }

}
