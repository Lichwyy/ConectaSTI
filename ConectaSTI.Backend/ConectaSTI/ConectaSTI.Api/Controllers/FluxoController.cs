using AutoMapper;
using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using ConectaSTI.Executor.Servicos;
using FGB.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ConectaSTI.Api.Controllers;

public class FluxoController : CrudControllerBase<Fluxo, Fluxo>
{
    //private IVersionarExecutor _versionarExecutor;
    private IFluxoExecutor _fluxoExecutor;

    public FluxoController(ServicoFluxo servico, IMapper mapper, IFluxoExecutor fluxoExecutor) : base(servico, mapper)
    {
        //_versionarExecutor = versionarExecutor;
        _fluxoExecutor = fluxoExecutor;
    }
    
    //[HttpPost("/salvarFluxo/{fluxoId:long}")]
    //public virtual IActionResult SalvarFluxo(long fluxoId)
    //{
    //    var fluxoVersionado = _versionarExecutor.Execute(fluxoId);
        
    //    return Ok(fluxoVersionado);
    //}

    [HttpPost("/executarfluxo/{fluxoId}")]
    public async Task<IActionResult> ExecutarFluxo(
        long fluxoId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] EntradaFluxoDTO entrada = null)
    {
        var resultado = await _fluxoExecutor.Executar(fluxoId, entrada);

        return Ok(resultado);
    }
}
