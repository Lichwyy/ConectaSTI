using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using FGB.IRepositorios;
using Microsoft.AspNetCore.Mvc;

namespace ConectaSTI.Api.Controllers;

public class EndPointController : CrudControllerBase<EndPoint, EndPoint>
{
    private readonly IRequestExecutor _request;
    private readonly IFunctionExecutor _executor;
    private readonly IStorageExecutor _storageExecutor;
    private readonly IFluxoExecutor _fluxoExecutor;
    private readonly IRepositorioConsulta _repositorioConsulta;
    
    public EndPointController(ServicoEndPoint servico, IMapper mapper, IRequestExecutor request, IRepositorioConsulta repositorioConsulta, IFunctionExecutor executor, IStorageExecutor storageExecutor, IFluxoExecutor fluxoExecutor) : base(servico, mapper)
    {
        _request = request;
        _executor = executor;
        _storageExecutor = storageExecutor;
        _fluxoExecutor = fluxoExecutor;
        _repositorioConsulta = repositorioConsulta;
    }

    [HttpPost("/testerequest/{noId}")]
    public IActionResult Testar(long noId)
    {
        var no = _repositorioConsulta.Consulta<No>(no => no.Id == noId).FirstOrDefault();

        return Ok(_request.EnviarRequisicao(no));
    }
    
    [HttpPost("/testefunction/{funcaoId}")]
    public IActionResult TestarFunction(long funcaoId, [FromBody] object dadoAnterior)
    {
        var funcao = _repositorioConsulta.Consulta<Funcao>(funcao => funcao.Id == funcaoId).FirstOrDefault();
        
        return Ok(_executor.Executar(funcao, dadoAnterior));
    }

    [HttpPost("/testestorage/{noId}")]
    public IActionResult TestarStorage(long noId, [FromBody] object dadoAnterior)
    {
        var no = _repositorioConsulta.Consulta<No>(no => no.Id == noId).FirstOrDefault();

        return Ok(_storageExecutor.Salvar(no));
    }
    
    [HttpPost("/testefluxo/{fluxoId}")]
    public async Task<IActionResult> TestarFluxo(long fluxoId) 
    {
        var resultado = await _fluxoExecutor.Executar(fluxoId);
    
        return Ok(resultado);
    }
}