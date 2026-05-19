using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using ConectaSTI.Executor.Servicos;
using FGB.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ConectaSTI.Api.Controllers;

public class FluxoController : CrudControllerBase<Fluxo, Fluxo>
{
    private IVersionarExecutor _versionarExecutor;
    private IFluxoExecutor _fluxoExecutor;

    public FluxoController(ServicoFluxo servico, IMapper mapper, IVersionarExecutor versionarExecutor, IFluxoExecutor fluxoExecutor) : base(servico, mapper)
    {
        _versionarExecutor = versionarExecutor;
        _fluxoExecutor = fluxoExecutor;
    }
    
    [HttpPost("/salvarFluxo/{fluxoId:long}")]
    public virtual IActionResult SalvarFluxo(long fluxoId)
    {
        var fluxoVersionado = _versionarExecutor.Execute(fluxoId);
        
        return Ok(fluxoVersionado);
    }

    [HttpPost("/executarfluxo/{fluxoId}")]
    public async Task<IActionResult> ExecutarFluxo(long fluxoId)
    {
        var resultado = await _fluxoExecutor.Executar(fluxoId);

        return Ok(resultado);
    }
}