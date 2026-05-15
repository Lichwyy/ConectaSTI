using System.Text.Encodings.Web;
using System.Text.Json;
using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.ObjetosValor;
using ConectaSTI.Dominio.Servicos;
using FGB.IRepositorios;

namespace ConectaSTI.Executor.Servicos;

public class VersionarExecutor :  IVersionarExecutor
{
    private readonly IRepositorioConsulta _consulta;
    private readonly ServicoFluxoVersionado _fluxoVersionado;

    public VersionarExecutor(IRepositorioConsulta consulta,  ServicoFluxoVersionado fluxoVersionado)
    {
        _consulta = consulta;
        _fluxoVersionado = fluxoVersionado;
    }
    
    public FluxoVersionado Execute(long fluxoId)
    {
        Fluxo fluxo = _consulta.Retorna<Fluxo>(fluxoId);

        int ultimaVersao = _consulta.Consulta<FluxoVersionado>(x => x.FluxoId == fluxoId).Select(x => (int?)x.Versao)
            .Max() ?? 0;
        int proximaVersao = ultimaVersao + 1;
        
        FluxoVersionado fluxoVersionado = new FluxoVersionado()
        {
            FluxoId = fluxoId,
            Nome = fluxo.Nome,
            Versao = proximaVersao,
        };
        
        var opcoes = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        FluxoDTO fluxoDto = new FluxoDTO()
        {
            Operacoes =  GetAllOperation(fluxo)
        };

        string fluxoSerializado = JsonSerializer.Serialize(fluxoDto, opcoes);
        
        fluxoVersionado.Payload = fluxoSerializado;
        
        _fluxoVersionado.Inclui(fluxoVersionado);
        
        return fluxoVersionado;
    }

    private List<OperacaoDTO> GetAllOperation(Fluxo fluxo)
    {
        List<OperacaoDTO> listaOperacao = new List<OperacaoDTO>();
        
        foreach (Operacao operacao in fluxo.Operacoes)
        {
            OperacaoDTO operacaoDto = new OperacaoDTO()
            {
                BackoffDelay =  operacao.BackoffDelay,
                BackoffMultiplier = operacao.BackoffMultiplier,
                BackoffType =  operacao.BackoffType,
                Erro =  operacao.Erro,
                MaximoRepeticao =  operacao.MaximoRepeticao,
                NoId = operacao.NoId,
                Ordem =  operacao.Ordem,
                Repetir =  operacao.Repetir,
                Timeout =   operacao.Timeout,
                UsarDadosAnterior =  operacao.UsarDadosAnterior
            };
            
            listaOperacao.Add(operacaoDto);
        }

        return listaOperacao;
    }
}