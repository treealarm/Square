using Domain;
using Microsoft.Extensions.Options;

namespace LeafletAlarms.Services
{
  public class FileSystemService
  {
    private readonly IOptions<RoutingSettings> _routingSettings;
    public FileSystemService(IOptions<RoutingSettings> routingSettings) 
    {
      _routingSettings = routingSettings;
    }
    public async Task<byte[]> GetFile(string mainFolder, string path, string file)
    {
      byte[] data = null;

      var dataDirectory = new DirectoryInfo(_routingSettings.Value.RootFolder);

      string basePath = AppDomain.CurrentDomain.BaseDirectory.ToString();

      if (dataDirectory.Exists)
      {
        basePath = dataDirectory.FullName;
      }

      basePath = Path.Combine(basePath, mainFolder, path);

      string localName = Path.Combine(basePath, file);

      if (File.Exists(localName))
      {
        try
        {
          data = await File.ReadAllBytesAsync(localName);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }

      return data;
    }
    public async Task<string> Upload(string mainFolder, string path, string fileName, byte[] file)
    {
        var dataDirectory = new DirectoryInfo(_routingSettings.Value.RootFolder);

        string basePath = AppDomain.CurrentDomain.BaseDirectory.ToString();

        if (dataDirectory.Exists)
        {
          basePath = dataDirectory.FullName;
        }

        basePath = Path.Combine(basePath, mainFolder, path);

        try
        {
          Directory.CreateDirectory(basePath);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }

        string filePath = Path.Combine(basePath, fileName);

      try
      {
        // Сохраняем файл на сервере
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await stream.WriteAsync(file);
        }
      }
      catch(Exception ex)
      { Console.WriteLine(ex.ToString()); }        

        // Возвращаем успешный результат загрузки
        return filePath;
    }
  }
}
