using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace OnvifLib
{
  public class MyMessageInspector : IClientMessageInspector
  {
    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
      // Логгирование ответа
      Console.WriteLine("Received Reply: ");
      Console.WriteLine(reply.ToString());
    }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
      // Логгирование запроса
      Console.WriteLine("Sending Request: ");
      Console.WriteLine(request.ToString());
      return null;
    }
  }

  public class MyClientBehavior : IClientMessageInspector, IEndpointBehavior
  {
    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
      throw new NotImplementedException();
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
      clientRuntime.ClientMessageInspectors.Add(new MyMessageInspector());
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
      throw new NotImplementedException();
    }

    public void Validate(ServiceEndpoint endpoint) { }
  }

}
