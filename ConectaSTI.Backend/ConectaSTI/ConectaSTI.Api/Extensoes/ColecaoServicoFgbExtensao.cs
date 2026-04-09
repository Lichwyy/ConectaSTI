using FGB.API.Utils;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.Repositorios;
using FGB.Dominio.Servicos;
using FGB.IRepositorios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using NHSession = NHibernate.ISession;
using NHSessionFactory = NHibernate.ISessionFactory;

namespace ConectaSTI.Api.Extensoes
{
    public static class ColecaoServicoFgbExtensao
    {
        public static IServiceCollection AddFgb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ =>
            {
                var connectionString = configuration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'ConnectionStrings:Default' não configurada. Defina via environment variable 'ConnectionStrings__Default' ou user-secrets.");
                }

                var cfg = new Configuration();
                cfg.Configure(Path.Combine(AppContext.BaseDirectory, "nhibernate.cfg.xml"));
                cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
                return cfg;
            });

            services.AddSingleton<NHSessionFactory>(sp =>
                sp.GetRequiredService<Configuration>().BuildSessionFactory());

            services.AddScoped<NHSession>(sp =>
                sp.GetRequiredService<NHSessionFactory>().OpenSession());

            services.AddTransient<IRepositorioSessao, RepositorioSessao>();
            services.AddTransient<IRepositorioConsulta, RepositorioConsulta>();
            services.AddTransient<IMigracao, Migracao>();
            services.AddScoped<IConverter, Conversor>();

            services.Configure<ServicoRequestOptions>(configuration.GetSection("ServicoRequest"));
            services.AddTransient<IRequest, ServicoRequest>();

            services.AddAutoMapperProfiles();

            return services;
        }
    }
}
