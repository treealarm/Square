using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  public class FilesController  : ControllerBase
  {
    private readonly string _imagesFolder = "static_files";
    private readonly IOptions<RoutingSettings> _routingSettings;
    public FilesController(  IOptions<RoutingSettings> routingSettings)
    {
      _routingSettings = routingSettings;
    }

    [AllowAnonymous]
    [HttpGet("GetFile/{path}/{file}")]
    public async Task<FileResult> GetFile(string path, string file)
    {
      byte[] data = null;

      var dataDirectory = new DirectoryInfo(_routingSettings.Value.RootFolder);

      string basePath = AppDomain.CurrentDomain.BaseDirectory.ToString();

      if (dataDirectory.Exists)
      {
        basePath = dataDirectory.FullName;
      }

      basePath = Path.Combine(basePath, _imagesFolder, path);

      string localName = Path.Combine(basePath, file);

      if (System.IO.File.Exists(localName))
      {
        try
        {
          data = await System.IO.File.ReadAllBytesAsync(localName);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }

      return data == null ?  File(data, "image/png") :null;
    }

    public class FileUploadModel
    {
      public IFormFile file { get; set; }
      public string path { get; set; }
    }

    [AllowAnonymous]
    [HttpPost("upload")]
    public async Task<ActionResult<string>> Upload(string path, IFormFile file)
    {
      try
      {
        // Проверяем, есть ли файл в запросе
        if (file == null || file.Length == 0)
        {
          return BadRequest("No file uploaded");
        }

        var dataDirectory = new DirectoryInfo(_routingSettings.Value.RootFolder);

        string basePath = AppDomain.CurrentDomain.BaseDirectory.ToString();

        if (dataDirectory.Exists)
        {
          basePath = dataDirectory.FullName;
        }

        basePath = Path.Combine(basePath, _imagesFolder, path);
        try
        {
          Directory.CreateDirectory(basePath);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }

        string filePath = Path.Combine(basePath, file.FileName);

        // Сохраняем файл на сервере
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }

        // Возвращаем успешный результат загрузки
        return Ok(new { message = "File uploaded successfully", filePath, path });
      }
      catch (Exception ex)
      {
        // Обрабатываем ошибку
        Console.WriteLine($"Error uploading file: {ex.Message}");
        return StatusCode(500, "Internal server error");
      }
    }
  }
}
