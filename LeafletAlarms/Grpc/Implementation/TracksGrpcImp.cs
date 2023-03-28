using Grpc.Core;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.TracksGrpcService;

namespace LeafletAlarms.Grpc.Implementation
{
  public class TracksGrpcImp: TracksGrpcServiceBase
  {
    private static int _counter = 1;
    private readonly ILogger<TracksGrpcImp> _logger;
    public TracksGrpcImp(ILogger<TracksGrpcImp> logger)
    {
      _logger = logger;
    }


    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
      Console.WriteLine($"Received from:{request.Name}");
      return Task.FromResult(new HelloReply
      {
        Message = "Hello " + request.Name + $"[{DateTime.Now}]"
      }); ;
    }
  }
}
