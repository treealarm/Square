using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OnvifLib
{


  class SimpleHttpServer
  {
    private readonly HttpListener _listener = new HttpListener();
    private bool _isRunning = false;

    public SimpleHttpServer(string prefix)
    {
      _listener.Prefixes.Add(prefix); // например "http://+:8080/onvif/events/"
    }

    public void Start()
    {
      _listener.Start();
      _isRunning = true;
      Console.WriteLine("HTTP Server started.");

      Task.Run(async () =>
      {
        while (_isRunning)
        {
          try
          {
            var context = await _listener.GetContextAsync();

            // Обработка запроса
            if (context.Request.HttpMethod == "POST")
            {
              using var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
              var body = await reader.ReadToEndAsync();

              Console.WriteLine("Received event push:\n" + body);

              // TODO: Здесь можно распарсить XML и вызвать обработчик

              var responseString = "OK";
              var buffer = Encoding.UTF8.GetBytes(responseString);
              context.Response.ContentLength64 = buffer.Length;
              context.Response.StatusCode = 200;
              await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
              context.Response.OutputStream.Close();
            }
            else
            {
              context.Response.StatusCode = 405; // Method Not Allowed
              context.Response.Close();
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine("Error processing HTTP request: " + ex);
          }
        }
      });
    }

    public void Stop()
    {
      _isRunning = false;
      _listener.Stop();
    }
  }

}
