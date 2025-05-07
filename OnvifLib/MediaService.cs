using System.Net;
using System.Net.Http.Headers;
using System.ServiceModel.Channels;
using System.Text;

namespace OnvifLib
{
  public class MediaService
  {
    public const string WSDL_V10 = "http://www.onvif.org/ver10/media/wsdl";
    public const string WSDL_V20 = "http://www.onvif.org/ver20/media/wsdl";

    protected readonly string _url;
    protected readonly string _username;
    protected readonly string _password;
    protected readonly string _profile;
    protected readonly CustomBinding _binding;

    protected MediaService(
      string url, 
      CustomBinding binding, 
      string username, 
      string password, 
      string profile)
    {
      _url = url;
      _username = username;
      _password = password;
      _binding = binding;
      _profile = profile;
    }

    public static async Task<MediaService> CreateAsync(
      string url, 
      CustomBinding binding, 
      string username, 
      string password,
      string profile)
    {
      if (profile == WSDL_V10)
      {
        var instance1 = new MediaService1(url, binding, username, password, profile);
        await instance1.InitializeAsync();
        return instance1;
      }
      
      var instance = new MediaService2(url, binding, username, password, profile);
      await instance.InitializeAsync();
      return instance;
    }

    public static async Task<byte[]> DownloadImageAsync(string url, string username, string password)
    {
      var handler = new HttpClientHandler
      {
        PreAuthenticate = true,
        Credentials = new NetworkCredential(username, password)
      };

      using var client = new HttpClient(handler);

      var response = await client.GetAsync(url);
      response.EnsureSuccessStatusCode();

      return await response.Content.ReadAsByteArrayAsync();
    }
    protected virtual async Task InitializeAsync() { await Task.CompletedTask; }
    public virtual async Task<byte[]?> GetImage()
    {
      await Task.CompletedTask;
      return null;
    }
  }
}
