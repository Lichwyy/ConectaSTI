using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FGB.Api.Extensions
{
    public static class AuthSecurityExtensions
    {
        public static IServiceCollection AddBifrostAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration["BifrostAuth:JwtKey"] ?? throw new InvalidOperationException("BifrostAuth:JwtKey não configurada.");

            var issuer = configuration["BifrostAuth:Issuer"] ?? throw new InvalidOperationException("BifrostAuth:Issuer não configurado.");

            var clientId = configuration["BifrostAuth:ClientId"] ?? throw new InvalidOperationException("BifrostAuth:ClientId não configurado.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.RequireHttpsMetadata = false; // só dev
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = clientId,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = "role",
                        NameClaimType = "name"
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperUser", policy => policy.RequireRole("superuser"));

                options.AddPolicy("admin", policy => {
                    // policy.RequireClaim("permission", "admin"); // somente teste
                    policy.RequireRole("admin");
                    }
                );
            });

            return services;
        }
    }
}
