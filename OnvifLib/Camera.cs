using System;
using System.Collections;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using DeviceServiceReference;
using MediaServiceReference;
using OnvifLib;


public class Camera
{
  private readonly string _url;
  private readonly string _username;
  private readonly string _password;
  private readonly DeviceClient _deviceClient;
  private readonly CustomBinding _binding;
  private Media2Client _mediaClient;
  public DeviceClient DeviceClient { get { return _deviceClient; } }


  //http://192.168.1.150:8899/onvif/device_service
  public Camera(string url, string username, string password)
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
    var services = await GetServices();

    if (services.TryGetValue("http://www.onvif.org/ver20/media/wsdl", out var url))
    {
      var endpoint = new EndpointAddress(url);
      _mediaClient = new Media2Client(_binding, endpoint);

      _mediaClient.ClientCredentials.UserName.UserName = _username;
      _mediaClient.ClientCredentials.UserName.Password = _password;
      _mediaClient.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
      _mediaClient.ClientCredentials.HttpDigest.ClientCredential.Password = _password;

      var behavior = new MyClientBehavior();
      _mediaClient.Endpoint.EndpointBehaviors.Add(behavior);

      await _mediaClient.OpenAsync();
      GetProfilesRequest request = new GetProfilesRequest();
      request.Type = ["All"];

      var profilesResponse = await _mediaClient.GetProfilesAsync(request);
      foreach (var profile in profilesResponse.Profiles)
      {
        GetStreamUriRequest streamUriRequest = new GetStreamUriRequest();
        streamUriRequest.Protocol = StreamType.RTPUnicast.ToString();
        streamUriRequest.ProfileToken = profile.token;
        var streamResponse = await _mediaClient.GetStreamUriAsync(streamUriRequest);
        Console.WriteLine($"Profile: {profile.Name}, RTSP URL: {streamResponse.Uri}");
      }
    }
  }
  async public Task<Dictionary<string, string>> GetServices()
  {
    var res = new Dictionary<string, string>();
    var result = await DeviceClient.GetServicesAsync(true);
    foreach (var service in result.Service)
    {
      res[service.Namespace] = service.XAddr;
    }
    return res;
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
