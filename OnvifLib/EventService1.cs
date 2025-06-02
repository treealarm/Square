using EventServiceReference1;
using OnvifLib;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Net;
using DeviceServiceReference;

public class EventService1
{
  public static string WSDL_V10 = "http://www.onvif.org/ver10/media/wsdl";
  protected readonly string _url;
  protected readonly string _username;
  protected readonly string _password;
  protected readonly string _profile;
  protected readonly CustomBinding _binding;

  private EventPortTypeClient? _eventClient1;
  private PullPointSubscriptionClient? _pullClient;
  private NotificationProducerClient? _notificationProducerClient1;
  private CancellationTokenSource? _cts;
  private string? _pullPointAddress;

  public event Action<NotificationMessageHolderType>? OnEventReceived;

  //SimpleHttpServer _httpServer = new SimpleHttpServer("http://10.3.1.150:5224/onvif/events/");

  protected EventService1(string url, CustomBinding binding, string username, string password, string profile)
  {
    _url = url;
    _username = username;
    _password = password;
    _binding = binding;
    _profile = profile;
    //_httpServer.Start();
  }

  public static async Task<EventService1?> CreateAsync(string url, CustomBinding binding, string username, string password, string profile)
  {
    var instance1 = new EventService1(url, binding, username, password, profile);
    await instance1.InitializeAsync();

    return instance1;
  }

  protected async Task InitializeAsync()
  {
    var endpoint = new EndpointAddress(_url);
    var clientInspector = new CustomMessageInspector();
    var behavior = new CustomEndpointBehavior(clientInspector);

    _eventClient1 = new EventPortTypeClient(_binding, endpoint);
    _eventClient1.ClientCredentials.UserName.UserName = _username;
    _eventClient1.ClientCredentials.UserName.Password = _password;
    _eventClient1.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
    _eventClient1.ClientCredentials.HttpDigest.ClientCredential.Password = _password;
    _eventClient1.Endpoint.EndpointBehaviors.Add(behavior);
    await _eventClient1.OpenAsync();


    var subscriptionResponse = await _eventClient1.CreatePullPointSubscriptionAsync(new CreatePullPointSubscriptionRequest
    {
      InitialTerminationTime = "PT1H"
    });

    _pullPointAddress = subscriptionResponse.SubscriptionReference.Address.Value;

    _pullClient = new PullPointSubscriptionClient(_binding, new EndpointAddress(_pullPointAddress));
    _pullClient.ClientCredentials.UserName.UserName = _username;
    _pullClient.ClientCredentials.UserName.Password = _password;
    _pullClient.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
    _pullClient.ClientCredentials.HttpDigest.ClientCredential.Password = _password;
    await _pullClient.OpenAsync();

    _notificationProducerClient1 = new NotificationProducerClient(_binding, endpoint);
    _notificationProducerClient1.ClientCredentials.UserName.UserName = _username;
    _notificationProducerClient1.ClientCredentials.UserName.Password = _password;
    _notificationProducerClient1.ClientCredentials.HttpDigest.ClientCredential.UserName = _username;
    _notificationProducerClient1.ClientCredentials.HttpDigest.ClientCredential.Password = _password;
    _notificationProducerClient1.Endpoint.EndpointBehaviors.Add(behavior);
    await _notificationProducerClient1.OpenAsync();    
  }

  public async Task  StartReceiving()
  {
    if (_pullClient == null) throw new InvalidOperationException("Pull client not initialized");

    _cts = new CancellationTokenSource();
    Task.Run(async () =>
    {
      await ReceiveLoopAsync(_cts.Token);
      await _pullClient.UnsubscribeAsync(new Unsubscribe());
    });

    var ip = "10.3.1.150";

     var subscr = new Subscribe()
     {
       InitialTerminationTime = "PT1H",
       ConsumerReference = new EndpointReferenceType()
       {
         Address = new AttributedURIType { Value = $"http://{ip}:5224/onvif/events/" }
       }
    };

    var subscriptionResponse = await _notificationProducerClient1!.SubscribeAsync(subscr);
  }

  public void StopReceiving()
  {
    _cts?.Cancel();
  }

  private async Task ReceiveLoopAsync(CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      try
      {
        if (_pullClient!.State != CommunicationState.Opened)
        {
          await _pullClient.OpenAsync();
        }

        var request = new PullMessagesRequest
        {
          Timeout = "PT1S",//XmlConvert.ToString(TimeSpan.FromSeconds(30)),
          MessageLimit = 1024
        };
        var response = await _pullClient!.PullMessagesAsync(request);

        foreach (var msg in response.NotificationMessage)
        {
          OnEventReceived?.Invoke(msg);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Pull failed: " + ex.Message);
        await Task.Delay(2000, token);
      }
    }
  }
}
