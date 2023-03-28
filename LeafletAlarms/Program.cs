using LeafletAlarms;
using LeafletAlarms.Grpc.Implementation;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

builder.Services.AddGrpc();
var app = builder.Build();
startup.Configure(app, builder.Environment);

// Configure the HTTP request pipeline.

app.MapGrpcService<TracksGrpcImp>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();