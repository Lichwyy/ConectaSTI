using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using ConectaSTI.Executor.Servicos;
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
            services.AddTransient<ServicoRota>();

            return services;
        }

        public static IServiceCollection AddExecutoresConectaSti(this IServiceCollection services)
        {
            services.AddTransient<IRequestExecutor, RequestExecutor>();
            services.AddTransient<IFunctionExecutor, FunctionExecutor>();
            services.AddTransient<IStorageExecutor, StorageExecutor>();
            services.AddTransient<IFluxoExecutor, FluxoVersionadoExecutor>();
            services.AddTransient<IVersionarExecutor, VersionarExecutor>();

            return services;
        }
    }
}
