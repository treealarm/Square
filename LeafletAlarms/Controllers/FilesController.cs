using Domain;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Mime;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  public class FilesController  : ControllerBase
  {
    private readonly string _imagesFolder = "static_files";
    private readonly IOptions<RoutingSettings> _routingSettings;
    private readonly FileSystemService _fs;
    public FilesController(
      IOptions<RoutingSettings> routingSettings,
      FileSystemService fs)
    {
      _routingSettings = routingSettings;
      _fs = fs;
    }

    [AllowAnonymous]
    [HttpGet("GetStaticFile/{path}/{file}")]
    public async Task<FileResult> GetStaticFile(string path, string file)
    {
      byte[] data = null;

      try
      {
        data = await _fs.GetFile(_imagesFolder, path, file);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);      }

      return data == null ?  File(data, MediaTypeNames.Application.Octet) :null;
    }

    [AllowAnonymous]
    [HttpGet("GetFile/{folder}/{path}/{file}")]
    public async Task<FileResult> GetFile(string folder, string path, string file)
    {
      byte[] data;

      try
      {
        data = await _fs.GetFile(folder, path, file);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        throw;
      }

      return File(data, MediaTypeNames.Application.Octet);
    }

    public class FileUploadModel
    {
      public IFormFile file { get; set; }
      public string path { get; set; }
    }

    [AllowAnonymous]
    [HttpPost("upload_static")]
    public async Task<ActionResult<string>> UploadStaticFile(string path, IFormFile file)
    {
      try
      {
        // Проверяем, есть ли файл в запросе
        if (file == null || file.Length == 0)
        {
          return BadRequest("No file uploaded");
        }
        string filePath = string.Empty;
        using (var item = new MemoryStream())
        {
          file.CopyTo(item);
          filePath = await _fs.Upload(_imagesFolder, path, file.FileName, item.ToArray());
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
