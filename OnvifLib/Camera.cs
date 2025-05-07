using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using DeviceServiceReference;
using OnvifLib;

public class Camera
{
  private readonly string _url;
  private readonly string _username;
  private readonly string _password;
  private readonly DeviceClient _deviceClient;
  private readonly CustomBinding _binding;
  private Dictionary<string, string>? _services;

  //http://192.168.1.150:8899/onvif/device_service
  public static async Task<Camera> CreateAsync(string ip, int port, string username, string password)
  {
    var url = $"http://{ip}:{port}/onvif/device_service";

    var camera = new Camera(url, username, password);
    await camera.Init();  // здесь ты можешь внутри запрашивать и кэшировать сервисы

    return camera;
  }
  private Camera(string url, string username, string password)
  {
    _url = url;
    _username = username;
    _password = password;

    // Настройка биндинга
    _binding = new CustomBinding(
      new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8), // SOAP 1.2
      new HttpTransportBindingElement()
      {
        AuthenticationScheme = AuthenticationSchemes.Digest
      }
    );

    // Создание адреса сервиса
    var endpoint = new EndpointAddress(url);

    // Инициализация клиента
    _deviceClient = new DeviceClient(_binding, endpoint);

    // Установка учетных данных
    _deviceClient.ClientCredentials.UserName.UserName = _username;
    _deviceClient.ClientCredentials.UserName.Password = _password;
    _deviceClient.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
    _deviceClient.ClientCredentials.HttpDigest.ClientCredential.Password = _password;
  }

  async public Task Init()
  {
    //Service: http://www.onvif.org/ver10/device/wsdl -> http://192.168.1.150:8899/onvif/device_service
    //Service: http://www.onvif.org/ver20/media/wsdl -> http://192.168.1.150:8899/onvif/media2_service
    //Service: http://www.onvif.org/ver10/media/wsdl -> http://192.168.1.150:8899/onvif/media_service
    //Service: http://www.onvif.org/ver10/events/wsdl -> http://192.168.1.150:8899/onvif/event_service
    //Service: http://www.onvif.org/ver20/ptz/wsdl -> http://192.168.1.150:8899/onvif/ptz_service
    //Service: http://www.onvif.org/ver20/imaging/wsdl -> http://192.168.1.150:8899/onvif/image_service
    //Service: http://www.onvif.org/ver20/analytics/wsdl -> http://192.168.1.150:8899/onvif/analytics_service
    //Service: http://www.onvif.org/ver10/deviceIO/wsdl -> http://192.168.1.150:8899/onvif/deviceIO_service
    _services = await GetServices();
  }

  public async Task<Dictionary<string, string>?> GetServicesAsync()
  {
    if (_services == null || _services.Count == 0)
    {
      _services = await GetServices();
    }

    return _services;
  }

  async public Task<MediaService?> GetMediaService()
  {
    var services = await GetServicesAsync();

    if (services != null)
    {
      var mediaWsdls = new[] {
          MediaService1.WSDL_V20,
          MediaService1.WSDL_V10
      };

      foreach (var wsdlKey in mediaWsdls)
      {
        if (services.TryGetValue(wsdlKey, out var url))
        {
          var mediaService = await MediaService1.CreateAsync(
            url, 
            _binding, 
            _username, 
            _password, 
            wsdlKey);
          if (mediaService != null)
          {
            return mediaService;
          }
        }
      }

    }

    return null;
  }
  async public Task<Dictionary<string, string>?> GetServices()
  {
    try
    {
      var res = new Dictionary<string, string>();
      var result = await _deviceClient.GetServicesAsync(true);
      foreach (var service in result.Service)
      {
        res[service.Namespace] = service.XAddr;
      }
      return res;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
    }
    return null;
  }

  /// <summary>
  /// Закрыть соединение
  /// </summary>
  public void Close()
  {
    try
    {
      _deviceClient.Close();
    }
    catch
    {
      _deviceClient.Abort();
    }
  }
}
