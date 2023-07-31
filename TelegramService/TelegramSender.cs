using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace TelegramService
{
  public class TelegramSender
  {
    private const string SEND_MESSAGE = "sendMessage";
    private const string SEND_VENUE = "sendVenue";
    private const string GET_UPDATES = "getUpdates";
    private const string GET_FILE = "getFile";
    private const string SEND_PHOTO = "sendPhoto";
    
    private string _apiBaseUrl = "https://api.telegram.org";

    public TelegramSender()
    {
    }

    public static bool AcceptAllCertificatePolicy(
      object sender,
      X509Certificate certificate,
      X509Chain chain,
      System.Net.Security.SslPolicyErrors sslPolicyErrors
    )
    {
      return true;
    }

    public async Task<string> Send(TelegramMessage telegramMsg)
    {
      string result = string.Empty;

      try
      {
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback += AcceptAllCertificatePolicy;

        string apiMethod;

        if (string.IsNullOrEmpty(telegramMsg.latitude) &&
          string.IsNullOrEmpty(telegramMsg.longitude
        )
        )
        {
          apiMethod = SEND_MESSAGE;
        }
        else
        {
          apiMethod = SEND_VENUE;
        }

        // URL Example:
        // "https://api.telegram.org/bot809045046:AAGtKxtDWu5teRGKW_Li8wFBQuJ-l4A9h38/getUpdates"
        string URL = $"{_apiBaseUrl}/bot{telegramMsg.bot_id}";

        using (HttpClient httpClient = new HttpClient())
        {
          httpClient.BaseAddress = new Uri(URL);

          
          httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

          HttpResponseMessage response = await httpClient.PostAsJsonAsync(apiMethod, telegramMsg);

          response.EnsureSuccessStatusCode();

          result = await response.Content.ReadAsStringAsync();
        }          

        return result;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }

      return result;
    }

    private async Task<string> UploadFilesToRemoteUrl(HttpClient request, string[] files, NameValueCollection formFields = null)
    {
      //string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

      //request.DefaultRequestHeaders.Accept.Add(
      //      new MediaTypeWithQualityHeaderValue($"multipart/form-data; boundary={boundary}"));

      request.DefaultRequestHeaders.ConnectionClose = true;
      var multiForm = new MultipartFormDataContent();


      if (formFields != null)
      {
        foreach (string key in formFields.Keys)
        {
          multiForm.Add(new StringContent(formFields[key]), key);
        }
      }

      for (int i = 0; i < files.Length; i++)
      {
        using (var fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
        {
          multiForm.Add(new StreamContent(fileStream), "photo", Path.GetFileName(files[i]));
        }
      }

      var result =  await request.PostAsync(SEND_PHOTO, multiForm);
      return await result.Content.ReadAsStringAsync();
    }

    public async Task SendPhoto(TelegramMessage msg)
    {
      try
      {
        if (!string.IsNullOrEmpty(msg.latitude) && !string.IsNullOrEmpty(msg.longitude))
        {
          await Send(msg);
        }

        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback += AcceptAllCertificatePolicy;

        string filePath = msg.photo;
        string URL = $"{_apiBaseUrl}/bot{msg.bot_id}";

        using (HttpClient httpClient = new HttpClient())
        {
          httpClient.BaseAddress = new Uri(URL);
          httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

          string[] variable_name = { filePath };
          NameValueCollection form = new NameValueCollection();
          form["chat_id"] = msg.chat_id;
          form["caption"] = msg.text;
          var result = await UploadFilesToRemoteUrl(httpClient, variable_name, form);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    public async Task<TelegramUpdate> GetUpdates(string botId, long? offset)
    {
      try
      {
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback += AcceptAllCertificatePolicy;

        string apiMethod = GET_UPDATES;

        // URL Example:
        // "https://api.telegram.org/bot809045046:AAGtKxtDWu5teRGKW_Li8wFBQuJ-l4A9h38/getUpdates"
        string URL = $"{_apiBaseUrl}/bot{botId}/";


        using (HttpClient httpClient = new HttpClient())
        {
          httpClient.BaseAddress = new Uri(URL);

          httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

          HttpResponseMessage response = await httpClient.GetAsync($"{apiMethod}?offset={offset}");

          response.EnsureSuccessStatusCode();

          var sResult = await response.Content.ReadAsStringAsync();
          var myDeserializedClass = JsonSerializer.Deserialize<TelegramUpdate>(sResult);

          return myDeserializedClass;
        }        
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }

      return null;
    }

    public async Task<(byte[] data, string fileNameRef)> GetPhoto(string botId, string file_id)
    {
      string fileNameRef = string.Empty;
      try
      {
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback += AcceptAllCertificatePolicy;

        string apiMethod = GET_FILE;

        string URL = $"{_apiBaseUrl}/bot{botId}";
        

        using (HttpClient httpClient = new HttpClient())
        {
          httpClient.BaseAddress = new Uri(URL);

          httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

          HttpResponseMessage response = await httpClient.GetAsync($"{apiMethod}?file_id={file_id}");

          response.EnsureSuccessStatusCode();

          var sResult = await response.Content.ReadAsStringAsync();
          var myDeserializedClass = JsonSerializer.Deserialize<GetFileResponse>(sResult);

          if (myDeserializedClass != null)
          {
            fileNameRef = Path.GetFileName(myDeserializedClass.result.file_path);

            URL = $"{_apiBaseUrl}/file/bot{botId}/{myDeserializedClass.result.file_path}";

            using (HttpClient client = new HttpClient())
            {
              return (await client.GetByteArrayAsync(URL), fileNameRef);
            }
          }
        }       
        
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }

      return (null, string.Empty);
    }
  }
}
