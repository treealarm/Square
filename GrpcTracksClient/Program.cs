// See https://aka.ms/new-console-template for more information
using System.Threading.Tasks;
using Dapr.Client;
using Grpc.Net.Client;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.TracksGrpcService;

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("http://localhost:5000");
var client = new TracksGrpcServiceClient(channel);

//var daprClient = new DaprClientBuilder().Build();

HelloReply reply = new HelloReply();
reply = await client.SayHelloAsync(
                  new HelloRequest { Name = "GreeterClient" });
Console.WriteLine("Greeting: " + reply.Message);

var figs = new ProtoFigures();
var fig = new ProtoFig();
figs.Figs.Add(fig);

fig.Id = "6423e54d513bfe83e9d59793";
fig.Name = "Test";
fig.Geometry = new ProtoGeometry();
fig.Geometry.Type = "Polygon";
fig.Geometry.Coord.Add(new ProtoCoord()
{
  Lat = 55.7566737398449,
  Lon = 37.60722931951715
});

fig.Geometry.Coord.Add(new ProtoCoord()
{
  Lat = 55.748852242908995,
  Lon = 37.60259563134112
});

fig.Geometry.Coord.Add(new ProtoCoord()
{
  Lat = 55.75203896803514,
  Lon = 37.618727730916895
});

//for (int i = 0; i < 100; i++)
{
  //try
  //{
  //  reply =
  //    await daprClient.InvokeMethodGrpcAsync<HelloRequest, HelloReply>(
  //      "server",
  //      "SayHello",
  //      new HelloRequest { Name = "GreeterClient" }
  //    );
  //}
  //catch (Exception ex)
  //{
  //  Console.WriteLine(ex);
  //}

  double step = 0.001;

  for (int i = 0; i < 100; i++)
  {
    await Task.Delay(1000);
    if (i > 50)
    {
      step = -0.001;
    }
    foreach(var f in fig.Geometry.Coord)
    {
      f.Lat += step;
      f.Lon += step;
    }
    var newFigs = await client.AddTracksAsync(figs);
  }
  

  
  //Console.WriteLine("Greeting: " + newFigs.ToString());
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
