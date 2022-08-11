﻿using Microsoft.Extensions.DependencyInjection;
using GCH.Infrastructure.TelegramBot.Services;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.Interfaces.Sources;
using GCH.Infrastructure.Voices;

namespace GCH.Infrastructure
{
    public static class CollectionServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IWrappedTelegramClient, WrappedTelegramClient>();
            services.AddScoped<IVoiceLabelSource, VoiceLabelSource>();
            return services;
        }
    }
}