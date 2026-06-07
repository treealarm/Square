using System;

namespace Domain
{
  /// <summary>
  /// Чтение обязательного конфига из окружения БЕЗ тихих фоллбэков:
  /// если переменная не задана — явно логируем и падаем (fail-fast),
  /// чтобы кривой/неполный деплой не маскировался дефолтами.
  /// </summary>
  public static class EnvConfig
  {
    public static string Require(string name)
    {
      var val = Environment.GetEnvironmentVariable(name);
      if (string.IsNullOrWhiteSpace(val))
      {
        var msg = $"Required environment variable '{name}' is not set.";
        Console.Error.WriteLine($"[FATAL CONFIG] {msg}");
        throw new InvalidOperationException(msg);
      }
      return val!;
    }
  }
}
