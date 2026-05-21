using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Interfaces;
using System.Threading.RateLimiting;

namespace ConectaSTI.Rotas.Middlewares
{
    public class RateLimitMiddleware : IMiddleware
    {
        private readonly RotaDTO _rotaDTO;
        private readonly IRateLimiterCache _rateLimiterCache;

        public RateLimitMiddleware(RotaDTO rotaDTO, IRateLimiterCache rateLimiterCache)
        {
            _rotaDTO = rotaDTO;
            _rateLimiterCache = rateLimiterCache;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_rotaDTO.Rota?.RateLimit == true)
            {
                var userKey = context.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrWhiteSpace(userKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Nao foi possivel identificar a origem da requisicao.",
                        statusCode = StatusCodes.Status401Unauthorized
                    });
                    return;
                }

                var intervaloSegundos = _rotaDTO.Rota.RateLimitInterval > 0 ? _rotaDTO.Rota.RateLimitInterval : 60;
                var permitLimit = _rotaDTO.Rota.RateLimitRequests > 0 ? _rotaDTO.Rota.RateLimitRequests : 10;
                var window = TimeSpan.FromSeconds(intervaloSegundos);
                var limiterKey = $"ratelimit:rota:{_rotaDTO.Rota.Id}:limit:{permitLimit}:window:{intervaloSegundos}:user:{userKey}";

                var limiter = _rateLimiterCache.GetOrCreate(limiterKey, permitLimit, window);

                using var lease = await limiter.AcquireAsync(1, context.RequestAborted);
                if (!lease.IsAcquired)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = intervaloSegundos.ToString();
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = $"Limite de {permitLimit} requisicoes excedido. Tente novamente em {intervaloSegundos} segundos.",
                        statusCode = StatusCodes.Status429TooManyRequests
                    });
                    return;
                }
            }

            await next(context);
        }
    }
}
