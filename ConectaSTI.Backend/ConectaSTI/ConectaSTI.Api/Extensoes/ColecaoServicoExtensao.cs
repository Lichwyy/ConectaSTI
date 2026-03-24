using ConectaSTI.Dominio.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace ConectaSTI.Api.Extensoes
{
    public static class ColecaoServicoExtensao
    {
        public static IServiceCollection AddServicosConectaSti(this IServiceCollection services)
        {
            services.AddScoped<ServicoEndPoint>();
            services.AddScoped<ServicoFluxo>();
            services.AddScoped<ServicoFuncao>();
            services.AddScoped<ServicoIntegracao>();
            services.AddScoped<ServicoNo>();
            services.AddScoped<ServicoOperacao>();

            return services;
        }
    }
}
