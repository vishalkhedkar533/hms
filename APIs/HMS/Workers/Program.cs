using CommonLibrary;
using Data;
using Models.DTO;
using Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ExcelProcessingWorker>();
builder.Services.AddSingleton<IBulkInsertService<AgentDto>>(
    new PostgresBulkInsertService<AgentDto>(
        builder.Configuration.GetConnectionString("Default")));
);
var host = builder.Build();
host.Run();
