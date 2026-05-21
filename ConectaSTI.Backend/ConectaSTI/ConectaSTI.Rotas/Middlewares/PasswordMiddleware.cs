using ConectaSTI.Dominio.DTOs;

namespace ConectaSTI.Rotas.Middlewares
{
    public class PasswordMiddleware : IMiddleware
    {
        private const string PasswordHeaderName = "X-Rota-Senha"; // Nome do header, meti um meio generico

        private readonly RotaDTO _rotaDTO;

        public PasswordMiddleware(RotaDTO rotaDTO)
        {
            _rotaDTO = rotaDTO;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (string.IsNullOrWhiteSpace(_rotaDTO.Rota?.SenhaAcesso))
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(PasswordHeaderName, out var senhaInformada) ||
                senhaInformada.ToString() != _rotaDTO.Rota.SenhaAcesso)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Você não possui autorização para acessar esta rota.",
                    statusCode = StatusCodes.Status401Unauthorized
                });
                return;
            }

            await next(context);
        }
    }
}
