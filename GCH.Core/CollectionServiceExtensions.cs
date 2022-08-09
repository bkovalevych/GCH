using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GCH.Core
{
    public static class CollectionServiceExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}