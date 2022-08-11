using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using GCH.Core;
using GCH.Core.Interfaces.BlobContainers;
using GCH.Core.Interfaces.Tables;
using GCH.Infrastructure;
using GCH.Infrastructure.BlobContainers;
using GCH.Infrastructure.OggReader;
using GCH.Infrastructure.Settings;
using GCH.Infrastructure.Tables;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            builder.Services.AddScoped<IVoicesContainer>(serviceProvider =>
                new VoicesContainer(
                    new BlobContainerClient(
                        configuration["BlobConnectionString"], "voices")));

            builder.Services.AddScoped<IUserVoicesContainer>(serviceProvider =>
                new UserVoicesContainer(
                    new BlobContainerClient(
                        configuration["BlobConnectionString"], "uservoices")));
            
            builder.Services.AddScoped(serviceProvider =>
                new TableClient(configuration["BlobConnectionString"], "voices"));

            builder.Services.AddScoped<IUserSettingsTable>(serviceProvider => new UserSettingsTable(
                new TableClient(configuration["BlobConnectionString"], "userSettings")));

            builder.Services.AddScoped(serviceProvider =>
                new QueueClient(configuration["BlobConnectionString"], "voices"));

            builder.Services.AddScoped(provider => new OggReaderService(
                configuration["FfmpegBin"],
                configuration["TempFolder"],
                provider.GetRequiredService<ILogger<OggReaderService>>()));
            builder.Services.AddInfrastructure();
            builder.Services.AddCore();
        }
    }
}
