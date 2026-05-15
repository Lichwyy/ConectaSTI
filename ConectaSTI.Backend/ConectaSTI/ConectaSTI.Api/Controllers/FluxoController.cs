using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ConectaSTI.Api.Controllers;

public class FluxoController : CrudControllerBase<Fluxo, Fluxo>
{
    private IVersionarExecutor _versionarExecutor;
    
    public FluxoController(ServicoFluxo servico, IMapper mapper, IVersionarExecutor versionarExecutor) : base(servico, mapper)
    {
        _versionarExecutor = versionarExecutor;
    }
    
    [HttpPost("{fluxoId:long}")]
    public virtual IActionResult Post(long fluxoId)
    {
        var fluxoVersionado = _versionarExecutor.Execute(fluxoId);
        
        return Ok(fluxoVersionado);
    }
}