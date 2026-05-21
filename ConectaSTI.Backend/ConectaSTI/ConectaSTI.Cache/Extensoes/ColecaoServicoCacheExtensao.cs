using ConectaSTI.Cache.Servicos;
using ConectaSTI.Dominio.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConectaSTI.Cache.Extensoes;

public static class ColecaoServicoCacheExtensao
{
    public static IServiceCollection AddConectaCache(this IServiceCollection services)
    {
        services.AddFusionCache();
        services.AddSingleton<IRouteCache, FusionRouteCache>();
        services.AddSingleton<IRateLimiterCache, FusionRateLimiterCache>();

        return services;
    }
}
