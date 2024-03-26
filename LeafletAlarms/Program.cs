using LeafletAlarms;
using LeafletAlarms.Grpc.Implementation;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

builder.Services.AddGrpc();
var app = builder.Build();
startup.Configure(app, builder.Environment);

app.MapGrpcService<TracksGrpcImp>();
//app.MapGrpcService<TracksDaprImp>();
app.Run();