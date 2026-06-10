using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.Dominio.ObjetoValor;
using FGB.IRepositorios;
using FGB.Servicos;
using System.Text.Json;

namespace ConectaSTI.Rotas.Middlewares
{
    public class ExecutionMiddleware : IMiddleware
    {
        private readonly RotaDTO _rotaDTO;
        private readonly IRequest _request;
        private readonly IConfiguration _configuration;
        private readonly IRepositorioConsulta _repositorioConsulta;

        public ExecutionMiddleware(RotaDTO rotaDTO, IRequest request, IConfiguration configuration, IRepositorioConsulta repositorioConsulta)
        {
            _rotaDTO = rotaDTO;
            _request = request;
            _configuration = configuration;
            _repositorioConsulta = repositorioConsulta;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string urlExecucao = $"{_configuration["ExecutionEngine"]?.TrimEnd('/')}/api/pipeline/{_rotaDTO.Rota.PipelineVersaoId}";

            if (_rotaDTO.Rota.UsarFluxoMaisAtual)
            {
                FluxoVersionado fluxoVersionado = _repositorioConsulta
                    .Consulta<FluxoVersionado>(x => x.Id == _rotaDTO.Rota.PipelineVersaoId)
                    .FirstOrDefault();

                if (fluxoVersionado == null)
                {
                    RespostaHttp<object> configuracaoInvalida = new RespostaHttp<object>
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Retorno =
                        {
                            new MensagemRetorno("A rota aponta para uma versao de fluxo inexistente.", true)
                        }
                    };

                    context.Response.StatusCode = configuracaoInvalida.Status;
                    await context.Response.WriteAsJsonAsync(configuracaoInvalida);
                    return;
                }

                urlExecucao = $"{_configuration["ExecutionEngine"]?.TrimEnd('/')}/executarfluxo/{fluxoVersionado.FluxoId}";
            }

            RequisicaoHttp requisicao = new RequisicaoHttp
            {
                Url = urlExecucao,
                Verbo = VerboHttp.POST,
                TimeoutSegundos = 300
            };

            EntradaFluxoDTO entradaFluxo = CriarEntradaFluxo();
            if (entradaFluxo != null)
            {
                requisicao.Body = entradaFluxo;
            }

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

        private EntradaFluxoDTO CriarEntradaFluxo()
        {
            EntradaFluxoDTO entrada = new EntradaFluxoDTO
            {
                RouteParams = _rotaDTO.RouteParams == null
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(_rotaDTO.RouteParams, StringComparer.OrdinalIgnoreCase),
                QueryParams = _rotaDTO.QueryParams == null
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(_rotaDTO.QueryParams, StringComparer.OrdinalIgnoreCase),
                Body = ConverterBody(_rotaDTO.Body)
            };

            return entrada.TemDados() ? entrada : null;
        }

        private static object ConverterBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(body);
                return document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return body;
            }
        }
    }
}
