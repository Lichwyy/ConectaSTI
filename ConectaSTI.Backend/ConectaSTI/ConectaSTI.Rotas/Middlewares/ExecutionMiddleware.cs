using ConectaSTI.Dominio.DTOs;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.Servicos;

namespace ConectaSTI.Rotas.Middlewares
{
    public class ExecutionMiddleware : IMiddleware
    {
        private readonly RotaDTO _rotaDTO;
        private readonly IRequest _request;
        private readonly IConfiguration _configuration;

        public ExecutionMiddleware(RotaDTO rotaDTO, IRequest request, IConfiguration configuration)
        {
            _rotaDTO = rotaDTO;
            _request = request;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            RequisicaoHttp requisicao = new RequisicaoHttp
            {
                Url = $"{_configuration["ExecutionEngine"]?.TrimEnd('/')}/api/pipeline/{_rotaDTO.Rota.PipelineVersaoId}",
                Verbo = VerboHttp.POST,
                TimeoutSegundos = 300
            };

            var resposta = await _request.PostAsync<RespostaHttp<object>>(requisicao);

            if (resposta?.Sucesso == true && resposta.Resposta != null)
            {
                context.Response.StatusCode = resposta.Resposta.Status > 0
                    ? resposta.Resposta.Status
                    : StatusCodes.Status200OK;

                await context.Response.WriteAsJsonAsync(resposta.Resposta);
                return;
            }

            RespostaHttp<object> falha = new RespostaHttp<object>
            {
                Status = resposta?.Status > 0 ? resposta.Status : StatusCodes.Status500InternalServerError,
                RespostaBody = resposta?.RespostaBody,
                Retorno = resposta?.Retorno ?? new List<MensagemRetorno>()
            };

            context.Response.StatusCode = falha.Status;
            await context.Response.WriteAsJsonAsync(falha);
        }
    }
}
