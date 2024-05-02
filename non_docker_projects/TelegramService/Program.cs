namespace TelegramService
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      var envVars = new Dictionary<string, string>();

      var environmentVariables = Environment.GetEnvironmentVariables();

      foreach (var variable in environmentVariables.Keys)
      {
        var key = variable.ToString().ToLower();

        if (
          key == "botid" ||
          key == "chatid")
        {
          envVars.Add(key, environmentVariables[variable].ToString());
        }        
      }

      var sender = new TelegramPoller(envVars["botid"], envVars["chatid"]);

      await sender.DoWork();
    }
  }
}