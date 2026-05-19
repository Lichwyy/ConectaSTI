using ConectaSTI.Dominio.Servicos;
using FGB.Api.Extensions;

namespace ConectaSTI.Api.Extensoes
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddAutenticacao(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBifrostAuth(configuration);

            return services;
        }
    }
}
