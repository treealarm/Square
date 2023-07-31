namespace TelegramService
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var envVars = new Dictionary<string, string>();

      var environmentVariables = Environment.GetEnvironmentVariables();

      foreach (var variable in environmentVariables.Keys)
      {
        var key = variable.ToString().ToLower();

        if (key == "botid")
        {
          envVars.Add(key, environmentVariables[variable].ToString());
        }        
      }

      var sender = new TelegramPoller("809045046:AAGtKxtDWu5teRGKW_Li8wFBQuJ-l4A9h38", "-1001550499013");

      while (true)
      {
        Task.Delay(5000).Wait();
      }
      
    }
  }
}