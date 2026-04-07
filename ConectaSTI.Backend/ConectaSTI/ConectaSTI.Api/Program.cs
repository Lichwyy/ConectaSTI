using ConectaSTI.Api.Extensoes;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.API.Utils;
using FGB.Dominio.Repositorios;
using FGB.Dominio.Servicos;
using FGB.IRepositorios;
using Microsoft.AspNetCore.OData;
using NHibernate.Cfg;
using System.Text.Json.Serialization;
using NHSession = NHibernate.ISession;
using NHSessionFactory = NHibernate.ISessionFactory;

namespace ConectaSTI.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers().AddOData(opt =>
            opt.Select().Filter().OrderBy().Count().Expand().SetMaxTop(1000)).AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddSingleton(_ =>
        {
            var cfg = new Configuration();
            cfg.Configure(Path.Combine(AppContext.BaseDirectory, "nhibernate.cfg.xml"));
            return cfg;
        });

        builder.Services.AddSingleton<NHSessionFactory>(sp =>
            sp.GetRequiredService<Configuration>().BuildSessionFactory());

        builder.Services.AddScoped<NHSession>(sp =>
            sp.GetRequiredService<NHSessionFactory>().OpenSession());

        builder.Services.AddTransient<IRepositorioSessao, RepositorioSessao>();
        builder.Services.AddTransient<IMigracao, Migracao>();

        builder.Services.Configure<ServicoRequestOptions>(builder.Configuration.GetSection("ServicoRequest"));
        builder.Services.AddTransient<IRequest, ServicoRequest>();

        builder.Services.AddAutoMapperProfiles();
        builder.Services.AddServicosConectaSti();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        builder.Services.AddSwaggerGen();
        
        var app = builder.Build();

        if (app.Configuration.GetValue<bool>("MigrateDb"))
        {
            using var scope = app.Services.CreateScope();
            var migracao = scope.ServiceProvider.GetRequiredService<IMigracao>();
            var migrationFolder = app.Configuration.GetValue<string>("MigrationFolder") ?? "Migracoes";
            migracao.UpdateDatabase(migrationFolder);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", $"API 1");
                c.RoutePrefix = string.Empty;
            });
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("AllowAll");

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}