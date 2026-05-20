using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Rotas.Interfaces;
using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Rotas.Middlewares
{
    public class RoutingMiddleware : IMiddleware
    {
        private readonly IRoutingService _routingService;
        private readonly RotaDTO _rotaDTO;

        public RoutingMiddleware(IRoutingService routingService, RotaDTO rotaDTO)
        {
            _routingService = routingService;
            _rotaDTO = rotaDTO;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!Enum.TryParse(context.Request.Method, true, out VerboHttp metodo))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Metodo HTTP nao suportado.",
                    statusCode = StatusCodes.Status405MethodNotAllowed
                });
                return;
            }

            string caminho = context.Request.Path.Value?.Trim('/') ?? string.Empty;
            var rota = _routingService.GetRoute(metodo, caminho, out var parametros);

            if (rota == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Rota nao encontrada.",
                    statusCode = StatusCodes.Status404NotFound
                });
                return;
            }

            _rotaDTO.Rota = rota;
            _rotaDTO.Headers = parametros;

            await next(context);
        }
    }
}
