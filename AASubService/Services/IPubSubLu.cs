using Dapr.Messaging.PublishSubscribe;
using Domain;
using PubSubLib;

namespace AASubService
{
  public interface ISubServiceLu : ISubService 
  {

  }

  public class SubServiceLu : SubService, ISubServiceLu
  {
    public SubServiceLu(DaprPublishSubscribeClient messagingClient) : base(messagingClient) 
    {
      _pubsub_name = "lukich";
    }
  }

  public interface IPubServiceLu : IPubService { }

  public class PubServiceLu : PubService, IPubServiceLu
  {
    public PubServiceLu() : base()
    {
      _pubsub_name = "lukich";
    }
  }
}
