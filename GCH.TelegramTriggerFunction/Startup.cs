using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using GCH.Core;
using GCH.Infrastructure;
using GCH.Infrastructure.Settings;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GCH.TelegramTriggerFunction.Startup))]
namespace GCH.TelegramTriggerFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.Configure<TelegramBotSettings>(
                it => configuration.GetSection(nameof(TelegramBotSettings))
                .Bind(it));

            builder.Services.AddScoped(serviceProvider =>
                new BlobContainerClient(configuration["BlobConnectionString"], "voices"));
            builder.Services.AddScoped(serviceProvider =>
                new TableClient(configuration["BlobConnectionString"], "voices"));
            builder.Services.AddScoped(serviceProvider =>
                new QueueClient(configuration["BlobConnectionString"], "voices"));
            builder.Services.AddInfrastructure();
            builder.Services.AddCore();
        }
    }
}
