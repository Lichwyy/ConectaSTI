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
        RespostaHttp<object> resposta = new RespostaHttp<object>();
        
        FluxoVersionado fluxoVersionado = _repositorioConsulta.Consulta<FluxoVersionado>(x => x.Id == fluxoId).FirstOrDefault();

        if (fluxoVersionado == null)
        {
            resposta.Status = 404;
            resposta.Retorno.Add(new MensagemRetorno($"Nao foi possivel achar o fluxo com id {fluxoId}", true));
            return resposta;
        }
        
        FluxoDTO fluxodto = DeserializeFluxoDTO(fluxoVersionado);
        return ExecuteOperation(fluxodto);
    }

    private RespostaHttp<object> ExecuteOperation(FluxoDTO fluxoDto)
    {
        object dadoAnterior = null;
        
        foreach (OperacaoDTO operacaoDto in fluxoDto.Operacoes)
        {
            No no = _repositorioConsulta.Consulta<No>(x => x.Id == operacaoDto.NoId).FirstOrDefault();
            
            dadoAnterior = ExecuteNo(no, dadoAnterior, operacaoDto.UsarDadosAnterior).Resposta;
        }

        return new RespostaHttp<object>()
        {
            Status =  200,
            Resposta = dadoAnterior
        };
    }

    private RespostaHttp<object> ExecuteNo(No no, object dadoAnterior, bool usarDadoAnterior)
    {
        RespostaHttp<object> resposta = new RespostaHttp<object>();
        
        switch (no.Tipo)
        {
            case TipoNo.Requisicao:
                if (usarDadoAnterior)
                {
                    no.Body = dadoAnterior.ToString();
                }
                resposta = _requestExecutor.EnviarRequisicao(no);
                break;
            case TipoNo.FuncaoJS:
                Funcao funcao = _repositorioConsulta.Consulta<Funcao>(x => x.Id == no.FuncaoId).FirstOrDefault();
                resposta = _functionExecutor.Executar(funcao, dadoAnterior);
                break;
            case TipoNo.SalvarStorage:
                no.Body = dadoAnterior.ToString();
                resposta = _storageExecutor.Salvar(no);
                break;
            case TipoNo.PegarStorage:
                resposta = _storageExecutor.Pegar(no.ChaveValor);
                break;
        }
        
        return resposta;
    }

    private FluxoDTO DeserializeFluxoDTO(FluxoVersionado fluxoVersionado)
    {
        return JsonSerializer.Deserialize<FluxoDTO>(fluxoVersionado.Payload);
    }
}