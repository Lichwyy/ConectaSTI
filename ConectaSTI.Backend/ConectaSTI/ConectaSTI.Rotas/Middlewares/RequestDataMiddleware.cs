using ConectaSTI.Dominio.DTOs;

namespace ConectaSTI.Rotas.Middlewares
{
    public class RequestDataMiddleware : IMiddleware
    {
        private readonly RotaDTO _rotaDTO;

        public RequestDataMiddleware(RotaDTO rotaDTO)
        {
            _rotaDTO = rotaDTO;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _rotaDTO.QueryParams = context.Request.Query
                .ToDictionary(q => q.Key, q => q.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            if (context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                _rotaDTO.Body = await reader.ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            await next(context);
        }
    }
}
