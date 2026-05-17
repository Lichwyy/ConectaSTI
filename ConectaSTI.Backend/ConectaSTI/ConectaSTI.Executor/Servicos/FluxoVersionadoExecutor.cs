using System.Text.Json;
using System.Diagnostics;
using System.Threading;
using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Entidades.Logs;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.ObjetosValor;
using ConectaSTI.Dominio.Servicos;
using FGB.Dominio.ObjetoValor;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Executor.Servicos;

public class FluxoVersionadoExecutor : IFluxoExecutor
{
    private readonly IRepositorioConsulta _repositorioConsulta;
    private readonly IRequestExecutor _requestExecutor;
    private readonly IFunctionExecutor _functionExecutor;
    private readonly IStorageExecutor _storageExecutor;
    private readonly ServicoLogFluxo _servicoLogFluxo;
    private readonly ServicoLogOperacao _servicoLogOperacao;
    private readonly Random _random = new Random();

    public FluxoVersionadoExecutor(
        IRepositorioConsulta repositorioConsulta,
        IRequestExecutor requestExecutor,
        IFunctionExecutor functionExecutor,
        IStorageExecutor storageExecutor,
        ServicoLogFluxo servicoLogFluxo,
        ServicoLogOperacao servicoLogOperacao)
    {
        _repositorioConsulta = repositorioConsulta;
        _requestExecutor = requestExecutor;
        _functionExecutor = functionExecutor;
        _storageExecutor = storageExecutor;
        _servicoLogFluxo = servicoLogFluxo;
        _servicoLogOperacao = servicoLogOperacao;
    }
    
    public async Task<RespostaHttp<object>> Executar(long fluxoId)
    {
        FluxoVersionado fluxoVersionado = _repositorioConsulta
            .Consulta<FluxoVersionado>(x => x.FluxoId == fluxoId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (fluxoVersionado == null)
        {
            return CreateErrorResponse(404, $"Nao foi possivel achar o fluxo com id {fluxoId}");
        }

        FluxoDTO fluxoDto = DeserializeFluxoDTO(fluxoVersionado);
        if (fluxoDto == null || fluxoDto.Operacoes == null || !fluxoDto.Operacoes.Any())
        {
            return CreateErrorResponse(400, "Payload do fluxo versionado invalido.");
        }

        LogFluxo logFluxo = new LogFluxo
        {
            FluxoId = fluxoVersionado.FluxoId,
            Versao = fluxoVersionado.Versao
        };

        if (!_servicoLogFluxo.Inclui(logFluxo))
        {
            return CreateErrorResponse(500, GetMensagensErro(_servicoLogFluxo.Mensagens, "Erro ao salvar log de fluxo."));
        }

        return await ExecuteOperation(fluxoDto, logFluxo.Id);
    }

    private async Task<RespostaHttp<object>> ExecuteOperation(FluxoDTO fluxoDto, long logFluxoId)
    {
        var operacoesOrdenadas = fluxoDto.Operacoes.OrderBy(x => x.Ordem).ToList();
        var noIds = operacoesOrdenadas.Select(x => x.NoId).Distinct().ToList();
        var nosPorId = _repositorioConsulta
            .Consulta<No>(x => noIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);

        object dadoAnterior = null;
        
        foreach (OperacaoDTO operacaoDto in operacoesOrdenadas)
        {
            if (!nosPorId.TryGetValue(operacaoDto.NoId, out No no))
            {
                return CreateErrorResponse(404, $"Nao foi possivel achar o no com id {operacaoDto.NoId}.");
            }

            int tentativas = 0;
            int atrasoTotalMs = 0;
            RespostaHttp<object> respostaNo = null;
            Exception ultimaException = null;
            DateTime iniciadoEm = DateTime.Now;
            DateTime finalizadoEm = iniciadoEm;

            do
            {
                tentativas++;
                ResultadoExecucaoOperacao resultadoExecucao = await ExecuteNoWithTimeout(no, dadoAnterior, operacaoDto.UsarDadosAnterior, operacaoDto.Timeout);
                respostaNo = resultadoExecucao.Resposta;
                ultimaException = resultadoExecucao.Exception;
                iniciadoEm = resultadoExecucao.IniciadoEm;
                finalizadoEm = resultadoExecucao.FinalizadoEm;
                bool sucesso = IsSuccessResponse(respostaNo);

                bool deveTentarNovamente = !sucesso && operacaoDto.Repetir && (tentativas) <= operacaoDto.MaximoRepeticao;
                if (deveTentarNovamente)
                {
                    int delay = CalcularAtraso(operacaoDto, tentativas);
                    if (delay > 0)
                    {
                        atrasoTotalMs += delay;
                        await Task.Delay(delay);
                    }
                }
                else
                {
                    break;
                }
            } while (true);

            RespostaHttp<object> respostaLog = RegistrarLogOperacao(
                logFluxoId,
                operacaoDto,
                no,
                dadoAnterior,
                respostaNo,
                tentativas,
                atrasoTotalMs,
                iniciadoEm,
                finalizadoEm,
                ultimaException);
            if (!IsSuccessResponse(respostaLog))
            {
                return respostaLog;
            }

            if (!IsSuccessResponse(respostaNo))
            {
                switch (operacaoDto.Erro)
                {
                    case TipoErro.FalharFluxo:
                        return respostaNo;
                    case TipoErro.ContinuarFluxo:
                        break;
                    case TipoErro.ExecutarCompensacao:
                        return CreateErrorResponse(501, "Compensacao ainda nao implementada para fluxo versionado.");
                    default:
                        return CreateErrorResponse(400, $"Tipo de erro invalido: {operacaoDto.Erro}");
                }
            }

            dadoAnterior = respostaNo.Resposta;
        }

        return new RespostaHttp<object>()
        {
            Status =  200,
            Resposta = dadoAnterior
        };
    }

    private async Task<ResultadoExecucaoOperacao> ExecuteNoWithTimeout(No no, object dadoAnterior, bool usarDadoAnterior, int timeout)
    {
        DateTime iniciadoEm = DateTime.Now;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            using CancellationTokenSource timeoutCts =
                timeout > 0
                    ? new CancellationTokenSource(timeout)
                    : new CancellationTokenSource();

            RespostaHttp<object> resposta =
                await ExecuteNo(no, dadoAnterior, usarDadoAnterior, timeoutCts.Token);

            stopwatch.Stop();

            return new ResultadoExecucaoOperacao(
                resposta,
                null,
                iniciadoEm,
                iniciadoEm.AddMilliseconds(stopwatch.ElapsedMilliseconds));
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();

            return new ResultadoExecucaoOperacao(
                CreateErrorResponse(408, $"Timeout no nó {no.Id}"),
                ex,
                iniciadoEm,
                iniciadoEm.AddMilliseconds(stopwatch.ElapsedMilliseconds));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new ResultadoExecucaoOperacao(
                CreateErrorResponse(500, ex.Message),
                ex,
                iniciadoEm,
                iniciadoEm.AddMilliseconds(stopwatch.ElapsedMilliseconds));
        }
    }

    private async Task<RespostaHttp<object>> ExecuteNo(No no, object dadoAnterior, bool usarDadoAnterior, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (no == null)
        {
            return CreateErrorResponse(404, "No nao encontrado.");
        }

        RespostaHttp<object> resposta = new RespostaHttp<object>();
        
        switch (no.Tipo)
        {
            case TipoNo.Requisicao:
                string headerOriginal = no.Headers;
                string bodyOriginal = no.Body;

                if (!string.IsNullOrWhiteSpace(no.Headers))
                {
                    no.Headers = InterpolarVariaveis(no.Headers, dadoAnterior);
                }

                if (!string.IsNullOrWhiteSpace(no.Body) && no.Body.Contains("{{"))
                {
                    no.Body = InterpolarVariaveis(no.Body, dadoAnterior);
                }

                if (usarDadoAnterior)
                {
                    no.Body = dadoAnterior?.ToString();
                }

                resposta = _requestExecutor.EnviarRequisicao(no, cancellationToken);

                no.Headers = headerOriginal;
                no.Body = bodyOriginal;
                break;
            case TipoNo.FuncaoJS:
                if (!no.FuncaoId.HasValue)
                {
                    return CreateErrorResponse(400, $"No {no.Id} nao possui FuncaoId.");
                }

                Funcao funcao = _repositorioConsulta.Consulta<Funcao>(x => x.Id == no.FuncaoId).FirstOrDefault();
                if (funcao == null)
                {
                    return CreateErrorResponse(404, $"Nao foi possivel achar a funcao com id {no.FuncaoId} para o no com id {no.Id}");
                }

                resposta = _functionExecutor.Executar(funcao, dadoAnterior, cancellationToken);
                break;
            case TipoNo.SalvarStorage:
                no.Body = dadoAnterior?.ToString();
                resposta = _storageExecutor.Salvar(no, cancellationToken);
                break;
            case TipoNo.PegarStorage:
                resposta = _storageExecutor.Pegar(no.ChaveValor, cancellationToken);
                break;
            case TipoNo.Fluxo:
                resposta = await Executar(no.FluxoId.Value);
                break;
            default:
                return CreateErrorResponse(400, $"Tipo de no invalido: {no.Tipo}");
        }
        
        return resposta;
    }

    private RespostaHttp<object> RegistrarLogOperacao(
        long logFluxoId,
        OperacaoDTO operacaoDto,
        No no,
        object dadoAnterior,
        RespostaHttp<object> respostaNo,
        int tentativas,
        int atrasoTotalMs,
        DateTime iniciadoEm,
        DateTime finalizadoEm,
        Exception exception)
    {
        EndPoint endPoint = null;
        Integracao integracao = null;

        if (no.EndPointId.HasValue)
        {
            endPoint = _repositorioConsulta.Consulta<EndPoint>(x => x.Id == no.EndPointId.Value).FirstOrDefault();
            if (endPoint != null)
            {
                integracao = _repositorioConsulta.Consulta<Integracao>(x => x.Id == endPoint.IntegracaoId).FirstOrDefault();
            }
        }

        LogOperacao logOperacao = new LogOperacao
        {
            LogFluxoId = logFluxoId,
            NoId = no.Id,
            FuncaoId = no.FuncaoId,
            EndPointId = no.EndPointId,
            Nome = integracao?.Nome,
            Url = integracao?.Url,
            Recurso = endPoint?.Recurso,
            Verbo = endPoint?.Verbo ?? default,
            Token = endPoint?.Token,
            Tipo = no.Tipo,
            Body = no.Body,
            Headers = no.Headers,
            ChaveValor = no.ChaveValor,
            TempoMinutoValidade = no.TempoMinutoValidade,
            Ordem = operacaoDto.Ordem,
            Repetir = operacaoDto.Repetir,
            UsarDadosAnterior = operacaoDto.UsarDadosAnterior,
            Erro = operacaoDto.Erro,
            MaximoRepeticao = operacaoDto.MaximoRepeticao,
            BackoffType = operacaoDto.BackoffType,
            BackoffDelay = operacaoDto.BackoffDelay,
            BackoffMultiplier = operacaoDto.BackoffMultiplier,
            Timeout = operacaoDto.Timeout,
            IniciadoEm = iniciadoEm,
            FinalizadoEm = finalizadoEm,
            DuracaoMs = (int)Math.Max((finalizadoEm - iniciadoEm).TotalMilliseconds, 0),
            TentativasRealizadas = tentativas,
            AtrasoTotalMs = atrasoTotalMs,
            StatusHttp = respostaNo?.Status ?? 0,
            Sucesso = IsSuccessResponse(respostaNo),
            DadoAnterior = SerializeSafe(dadoAnterior),
            Resposta = SerializeSafe(respostaNo?.Resposta),
            RespostaBody = respostaNo?.RespostaBody,
            MensagensRetorno = SerializeSafe(respostaNo?.Retorno),
            MensagemErro = ExtractMensagemErro(respostaNo, exception),
            ExceptionTipo = exception?.GetType().FullName,
            StackTrace = exception?.ToString()
        };

        if (!_servicoLogOperacao.Inclui(logOperacao))
        {
            return CreateErrorResponse(500, GetMensagensErro(_servicoLogOperacao.Mensagens, "Erro ao salvar log da operacao."));
        }

        return new RespostaHttp<object> { Status = 200 };
    }

    private FluxoDTO DeserializeFluxoDTO(FluxoVersionado fluxoVersionado)
    {
        using JsonDocument payload = JsonDocument.Parse(fluxoVersionado.Payload);

        if (payload.RootElement.ValueKind == JsonValueKind.Array)
        {
            List<OperacaoDTO> operacoes = JsonSerializer.Deserialize<List<OperacaoDTO>>(fluxoVersionado.Payload);
            return new FluxoDTO
            {
                Operacoes = operacoes ?? new List<OperacaoDTO>()
            };
        }

        return JsonSerializer.Deserialize<FluxoDTO>(fluxoVersionado.Payload);
    }

    private int CalcularAtraso(OperacaoDTO operacaoDto, int tentativas)
    {
        return operacaoDto.BackoffType switch
        {
            BackoffType.Exponential => (int)(operacaoDto.BackoffDelay * Math.Pow(operacaoDto.BackoffMultiplier, tentativas - 1)),
            BackoffType.Jitter => _random.Next(0, Math.Max(operacaoDto.BackoffDelay, 1)),
            _ => operacaoDto.BackoffDelay
        };
    }

    private static bool IsSuccessResponse(RespostaHttp<object> resposta)
    {
        return resposta != null && resposta.Status >= 200 && resposta.Status < 300;
    }

    private static string GetMensagensErro(ListaMensagens mensagens, string fallback)
    {
        if (mensagens == null || mensagens.Count == 0)
        {
            return fallback;
        }

        return string.Join(" | ", mensagens.Select(x => x.Mensagem));
    }

    private static string SerializeSafe(object valor)
    {
        if (valor == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(valor);
        }
        catch (JsonException)
        {
            return valor.ToString();
        }
        catch (NotSupportedException)
        {
            return valor.ToString();
        }
    }

    private static string ExtractMensagemErro(RespostaHttp<object> resposta, Exception exception)
    {
        if (exception != null)
        {
            return exception.Message;
        }

        if (resposta?.Retorno == null || resposta.Retorno.Count == 0)
        {
            return null;
        }

        MensagemRetorno mensagemErro = resposta.Retorno.FirstOrDefault(x => x.Erro);
        return mensagemErro?.Mensagem;
    }

    private RespostaHttp<object> CreateErrorResponse(int status, string mensagem)
    {
        return new RespostaHttp<object>
        {
            Status = status,
            Retorno =
            {
                new MensagemRetorno(mensagem, true)
            }
        };
    }

    private sealed class ResultadoExecucaoOperacao
    {
        public ResultadoExecucaoOperacao(RespostaHttp<object> resposta, Exception exception, DateTime iniciadoEm, DateTime finalizadoEm)
        {
            Resposta = resposta;
            Exception = exception;
            IniciadoEm = iniciadoEm;
            FinalizadoEm = finalizadoEm;
        }

        public RespostaHttp<object> Resposta { get; }
        public Exception Exception { get; }
        public DateTime IniciadoEm { get; }
        public DateTime FinalizadoEm { get; }
    }

    private string InterpolarVariaveis(string textoAlvo, object dadoAnterior)
    {
        if (string.IsNullOrWhiteSpace(textoAlvo) || dadoAnterior == null)
            return textoAlvo;

        try
        {
            string jsonString = dadoAnterior is string str ? str : SerializeSafe(dadoAnterior);
            using JsonDocument document = JsonDocument.Parse(jsonString);

            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                string textoResultante = textoAlvo;

                foreach (var propriedade in document.RootElement.EnumerateObject())
                {
                    string marcador = "{{" + propriedade.Name + "}}";
                    if (textoResultante.Contains(marcador))
                    {
                        string valorParaSubstituir = propriedade.Value.ValueKind == JsonValueKind.String
                            ? propriedade.Value.GetString()
                            : propriedade.Value.GetRawText();

                        textoResultante = textoResultante.Replace(marcador, valorParaSubstituir);
                    }
                }
                return textoResultante;
            }
        }
        catch
        {
        }

        return textoAlvo;
    }
}