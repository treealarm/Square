using PtzServiceReference;
using System.ServiceModel.Channels;

namespace OnvifLib
{
  public class PtzService2 : OnvifServiceBase, IOnvifServiceFactory<PtzService2>
  {
    public const string WSDL_V20 = "http://www.onvif.org/ver20/ptz/wsdl";
    private PTZClient? _ptzClient;

    protected PtzService2(string url, CustomBinding binding, string username, string password, string profile) :
      base(url, binding, username, password, profile)
    {
    }

    public static string[] GetSupportedWsdls()
    {
      return new[] { WSDL_V20 };
    }
    public static async Task<PtzService2?> CreateAsync(string url, CustomBinding binding, string username, string password, string profile)
    {
      var instance = new PtzService2(url, binding, username, password, profile);
      await instance.InitializeAsync();
      return instance;
    }

    protected async Task InitializeAsync()
    {
      _ptzClient = OnvifClientFactory.CreateClient<PTZClient, PTZ>(_url, _binding, _username, _password);
      await _ptzClient.OpenAsync();
    }

    public async Task ContinuousMoveAsync(string profileToken, float panTiltX, float panTiltY, float zoom = 0f, string timeout = "PT0S")
    {
      if (_ptzClient == null)
        throw new InvalidOperationException("PTZ client not initialized");

      var velocity = new PTZSpeed
      {
        PanTilt = new Vector2D { x = panTiltX, y = panTiltY },
        Zoom = new Vector1D { x = zoom }
      };

      await _ptzClient.ContinuousMoveAsync(profileToken, velocity, timeout);
    }


    public async Task StopAsync(string profileToken, bool panTilt = true, bool zoom = true)
    {
      if (_ptzClient == null)
        throw new InvalidOperationException("PTZ client not initialized");

      await _ptzClient.StopAsync(profileToken, panTilt, zoom);
    }

    public override void Dispose()
    {
      try { _ptzClient?.Close(); } catch { }
      base.Dispose();
    }
  }
}
