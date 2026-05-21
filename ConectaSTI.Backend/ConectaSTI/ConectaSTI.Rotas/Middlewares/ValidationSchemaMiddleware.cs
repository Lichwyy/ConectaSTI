using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Rotas.Interfaces;

namespace ConectaSTI.Rotas.Middlewares
{
    public class ValidationSchemaMiddleware : IMiddleware
    {
        private readonly ISchemaValidator _schemaValidator;
        private readonly RotaDTO _rotaDTO;

        public ValidationSchemaMiddleware(ISchemaValidator schemaValidator, RotaDTO rotaDTO)
        {
            _schemaValidator = schemaValidator;
            _rotaDTO = rotaDTO;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (HttpMethods.IsPost(context.Request.Method) ||
                HttpMethods.IsPut(context.Request.Method) ||
                HttpMethods.IsPatch(context.Request.Method))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);

                if (!string.IsNullOrWhiteSpace(_rotaDTO.Rota?.JsonSchemaReq))
                {
                    var jsonToValidate = string.IsNullOrWhiteSpace(body) ? "{}" : body;
                    var errors = _schemaValidator.ValidateJsonAgainstSchema(jsonToValidate, _rotaDTO.Rota.JsonSchemaReq);

                    if (errors.Count > 0)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "JSON invalido na requisicao.",
                            statusCode = StatusCodes.Status400BadRequest,
                            errors
                        });
                        return;
                    }
                }
            }

            await next(context);
        }
    }
}
