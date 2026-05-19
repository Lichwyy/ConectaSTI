using ConectaSTI.Dominio.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace ConectaSTI.Api.Extensoes
{
    public static class ColecaoServicoExtensao
    {
        public static IServiceCollection AddServicosConectaSti(this IServiceCollection services)
        {
            services.AddTransient<ServicoEndPoint>();
            services.AddTransient<ServicoFluxo>();
            services.AddTransient<ServicoFuncao>();
            services.AddTransient<ServicoIntegracao>();
            services.AddTransient<ServicoNo>();
            services.AddTransient<ServicoOperacao>();
            services.AddTransient<ServicoStorage>();
            services.AddTransient<ServicoFluxoVersionado>();
            services.AddTransient<ServicoLogFluxo>();
            services.AddTransient<ServicoLogOperacao>();

            return services;
        }
    }
}
