using System.Text.Json;
using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.ObjetosValor;
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

    public FluxoVersionadoExecutor(IRepositorioConsulta repositorioConsulta,  IRequestExecutor requestExecutor,  IFunctionExecutor functionExecutor, IStorageExecutor storageExecutor)
    {
        _repositorioConsulta = repositorioConsulta;
        _requestExecutor = requestExecutor;
        _functionExecutor = functionExecutor;
        _storageExecutor = storageExecutor;
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

        if (fluxoDto == null)
        {
            return CreateErrorResponse(400, "Payload do fluxo versionado invalido.");
        }

        return ExecuteOperation(fluxoDto);
    }

    private RespostaHttp<object> ExecuteOperation(FluxoDTO fluxoDto)
    {
        if (fluxoDto.Operacoes == null || !fluxoDto.Operacoes.Any())
        {
            return CreateErrorResponse(400, "Fluxo versionado nao possui operacoes.");
        }

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

            RespostaHttp<object> respostaNo = ExecuteNo(no, dadoAnterior, operacaoDto.UsarDadosAnterior);

            if (respostaNo.Status < 200 || respostaNo.Status >= 300)
            {
                return respostaNo;
            }

            dadoAnterior = respostaNo.Resposta;
        }

        return new RespostaHttp<object>()
        {
            Status =  200,
            Resposta = dadoAnterior
        };
    }

    private RespostaHttp<object> ExecuteNo(No no, object dadoAnterior, bool usarDadoAnterior)
    {
        if (no == null)
        {
            return CreateErrorResponse(404, "No nao encontrado.");
        }

        RespostaHttp<object> resposta = new RespostaHttp<object>();
        
        switch (no.Tipo)
        {
            case TipoNo.Requisicao:
                if (usarDadoAnterior)
                {
                    no.Body = dadoAnterior?.ToString();
                }
                resposta = _requestExecutor.EnviarRequisicao(no);
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

                resposta = _functionExecutor.Executar(funcao, dadoAnterior);
                break;
            case TipoNo.SalvarStorage:
                no.Body = dadoAnterior?.ToString();
                resposta = _storageExecutor.Salvar(no);
                break;
            case TipoNo.PegarStorage:
                resposta = _storageExecutor.Pegar(no.ChaveValor);
                break;
            default:
                return CreateErrorResponse(400, $"Tipo de no invalido: {no.Tipo}");
        }
        
        return resposta;
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
}