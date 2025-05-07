using MediaServiceReference;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OnvifLib
{
  public class MediaService
  {
    private Media2Client? _mediaClient;
    private readonly string _url;
    private readonly string _username;
    private readonly string _password;
    private readonly CustomBinding _binding;
    private List<MediaProfile> _profiles = new();

    private MediaService(string url, CustomBinding binding, string username, string password)
    {
      _url = url;
      _username = username;
      _password = password;
      _binding = binding;
    }

    public static async Task<MediaService> CreateAsync(string url, CustomBinding binding, string username, string password)
    {
      var instance = new MediaService(url, binding, username, password);
      await instance.InitializeAsync();
      return instance;
    }

    private async Task InitializeAsync()
    {
      var endpoint = new EndpointAddress(_url);
      _mediaClient = new Media2Client(_binding, endpoint);

      _mediaClient.ClientCredentials.UserName.UserName = _username;
      _mediaClient.ClientCredentials.UserName.Password = _password;
      _mediaClient.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
      _mediaClient.ClientCredentials.HttpDigest.ClientCredential.Password = _password;

      var behavior = new MyClientBehavior();
      _mediaClient.Endpoint.EndpointBehaviors.Add(behavior);

      await _mediaClient.OpenAsync();
      var request = new GetProfilesRequest { Type = ["All"] };
      var profilesResponse = await _mediaClient.GetProfilesAsync(request);

      _profiles = profilesResponse.Profiles.ToList();

      foreach (var profile in _profiles)
      {
        var streamUriRequest = new GetStreamUriRequest
        {
          Protocol = StreamType.RTPUnicast.ToString(),
          ProfileToken = profile.token
        };

        var streamResponse = await _mediaClient.GetStreamUriAsync(streamUriRequest);
        Console.WriteLine($"Profile: {profile.Name}, RTSP URL: {streamResponse.Uri}");
      }
    }

    public async Task<string?> GetImage()
    {
      var profile = _profiles.FirstOrDefault();
      if (profile == null || _mediaClient == null)
        return null;

      var snapShotUriRequest = new GetSnapshotUriRequest
      {
        ProfileToken = profile.token
      };

      var snapShotUriResponse = await _mediaClient.GetSnapshotUriAsync(snapShotUriRequest);
      return snapShotUriResponse.Uri;
    }
  }
}
