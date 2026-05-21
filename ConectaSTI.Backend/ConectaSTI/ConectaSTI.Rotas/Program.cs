using ConectaSTI.Cache.Extensoes;
using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Rotas.Interfaces;
using ConectaSTI.Rotas.Middlewares;
using ConectaSTI.Rotas.Servicos;
using FGB.Dominio.Entidades;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.Repositorios;
using FGB.Dominio.Servicos;
using FGB.IRepositorios;
using FGB.Servicos;
using Microsoft.AspNetCore.HttpOverrides;
using NHibernate.Cfg;
using System.Net;
using NHSession = NHibernate.ISession;
using NHSessionFactory = NHibernate.ISessionFactory;

namespace ConectaSTI.Rotas;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton(_ =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'ConnectionStrings:Default' nao configurada.");
            }

            var cfg = new Configuration();
            cfg.Configure(Path.Combine(AppContext.BaseDirectory, "nhibernate.cfg.xml"));
            cfg.AddAssembly(typeof(LogEntidade).Assembly);
            cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
            return cfg;
        });

        builder.Services.AddSingleton<NHSessionFactory>(sp =>
            sp.GetRequiredService<Configuration>().BuildSessionFactory());

        builder.Services.AddScoped<NHSession>(sp =>
            sp.GetRequiredService<NHSessionFactory>().OpenSession());

        builder.Services.AddTransient<IRepositorioSessao, RepositorioSessao>();
        builder.Services.AddTransient<IRepositorioConsulta, RepositorioConsulta>();
        builder.Services.AddTransient<IMigracao, Migracao>();
        builder.Services.AddScoped<IConverter, Conversor>();

        builder.Services.Configure<ServicoRequestOptions>(builder.Configuration.GetSection("ServicoRequest"));
        builder.Services.AddTransient<IRequest, ServicoRequest>();
        builder.Services.AddConectaCache();
        builder.Services.Configure<ForwardedHeadersOptions>(options => ConfigureForwardedHeaders(options, builder.Configuration));

        builder.Services.AddScoped<RotaDTO>();

        builder.Services.AddTransient<IRoutingService, RoutingService>();
        builder.Services.AddTransient<ISchemaValidator, JsonValidator>();

        builder.Services.AddScoped<RoutingMiddleware>();
        builder.Services.AddScoped<RateLimitMiddleware>();
        builder.Services.AddScoped<PasswordMiddleware>();
        builder.Services.AddScoped<ValidationSchemaMiddleware>();
        builder.Services.AddScoped<RequestDataMiddleware>();
        builder.Services.AddScoped<ExecutionMiddleware>();

        var app = builder.Build();

        if (app.Configuration.GetValue<bool>("MigrateDb"))
        {
            using var scope = app.Services.CreateScope();
            var migracao = scope.ServiceProvider.GetRequiredService<IMigracao>();
            var migrationFolder = app.Configuration.GetValue<string>("MigrationFolder") ?? "Migracoes";
            migracao.UpdateDatabase(migrationFolder);
        }

        app.UseForwardedHeaders();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<RoutingMiddleware>();
        app.UseMiddleware<RateLimitMiddleware>();
        app.UseMiddleware<PasswordMiddleware>();
        app.UseMiddleware<ValidationSchemaMiddleware>();
        app.UseMiddleware<RequestDataMiddleware>();
        app.UseMiddleware<ExecutionMiddleware>();

        app.Run();
    }

    private static void ConfigureForwardedHeaders(ForwardedHeadersOptions options, IConfiguration configuration)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();

        foreach (var proxy in configuration.GetSection("ForwardedHeaders:KnownProxies").GetChildren())
        {
            if (IPAddress.TryParse(proxy.Value, out var ipAddress))
            {
                options.KnownProxies.Add(ipAddress);
            }
        }

        foreach (var network in configuration.GetSection("ForwardedHeaders:KnownNetworks").GetChildren())
        {
            if (TryParseNetwork(network.Value, out var ipNetwork))
            {
                options.KnownIPNetworks.Add(ipNetwork);
            }
        }
    }

    private static bool TryParseNetwork(string value, out System.Net.IPNetwork network)
    {
        network = default!;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var partes = value.Split('/', 2, StringSplitOptions.TrimEntries);
        if (partes.Length != 2)
        {
            return false;
        }

        if (!IPAddress.TryParse(partes[0], out var prefix))
        {
            return false;
        }

        if (!int.TryParse(partes[1], out var prefixLength))
        {
            return false;
        }

        try
        {
            network = new System.Net.IPNetwork(prefix, prefixLength);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
