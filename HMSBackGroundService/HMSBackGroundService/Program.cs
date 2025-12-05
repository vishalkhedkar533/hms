using HMSBackGroundService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<AgentCreate>();

var host = builder.Build();
host.Run();
