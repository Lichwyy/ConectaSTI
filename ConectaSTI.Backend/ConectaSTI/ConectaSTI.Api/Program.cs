using ConectaSTI.Api.Extensoes;
using FGB.IRepositorios;
using Microsoft.AspNetCore.OData;
using System.Text.Json.Serialization;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Executor.Servicos;

namespace ConectaSTI.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers(opt =>
                {
                    opt.InputFormatters.Insert(0, new PlainTextInputFormatter());
                })
            .AddOData(opt =>
            opt.Select().Filter().OrderBy().Count().Expand().SetMaxTop(1000)).AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddFgb(builder.Configuration);
        builder.Services.AddServicosConectaSti();

        builder.Services.AddTransient<IRequestExecutor, RequestExecutor>();
        builder.Services.AddTransient<IFunctionExecutor, FunctionExecutor>();
        builder.Services.AddTransient<IStorageExecutor, StorageExecutor>();
        builder.Services.AddTransient<IFluxoExecutor, FluxoVersionadoExecutor>();
        builder.Services.AddTransient<IVersionarExecutor, VersionarExecutor>();

        builder.Services.AddCors(options =>  //depois configuramos direito, enquanto estiver em desenvolvimento, deixamos aberto
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