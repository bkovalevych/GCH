using Microsoft.Extensions.DependencyInjection;
using GCH.Infrastructure.TelegramBot.Services;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.Interfaces.Sources;
using GCH.Infrastructure.Voices;
using GCH.Core.Interfaces.FfmpegHelpers;
using GCH.Infrastructure.OggReader;

namespace GCH.Infrastructure
{
    public static class CollectionServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IWrappedTelegramClient, WrappedTelegramClient>();
            services.AddScoped<IVoiceLabelSource, VoiceLabelSource>();
            services.AddScoped<IOggReaderService, OggReaderService>();
            return services;
        }
    }
}