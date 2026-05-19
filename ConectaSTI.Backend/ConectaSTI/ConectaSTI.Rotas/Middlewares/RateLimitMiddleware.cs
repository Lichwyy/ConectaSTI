using ConectaSTI.Dominio.DTOs;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace ConectaSTI.Rotas.Middlewares
{
    public class RateLimitMiddleware : IMiddleware
    {
        private static readonly ConcurrentDictionary<string, FixedWindowRateLimiter> _limiters = new();

        private readonly RotaDTO _rotaDTO;

        public RateLimitMiddleware(RotaDTO rotaDTO)
        {
            _rotaDTO = rotaDTO;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_rotaDTO.Rota?.RateLimit == true)
            {
                var userKey = context.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrWhiteSpace(userKey))
                {
                    userKey = context.Request.Headers["X-Forwarded-For"].ToString();
                }

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
                var limiterKey = $"ratelimit:{_rotaDTO.Rota.Id}:user:{userKey}";

                var limiter = _limiters.GetOrAdd(limiterKey, _ => new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(intervaloSegundos),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

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
