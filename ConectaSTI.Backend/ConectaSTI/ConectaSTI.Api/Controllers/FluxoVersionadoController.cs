using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ConectaSTI.Api.Controllers;

public class FluxoVersionadoController : ConsultaControllerBase<FluxoVersionado, FluxoVersionado>
{
    private readonly IFluxoExecutor _fluxoExecutor;

    public FluxoVersionadoController(ServicoFluxoVersionado servico, IMapper mapper, IFluxoExecutor fluxoExecutor)
        : base(servico, mapper)
    {
        _fluxoExecutor = fluxoExecutor;
    }

    [HttpPost("/api/pipeline/{fluxoVersionadoId:long}")]
    public async Task<IActionResult> ExecutarFluxoVersionado(long fluxoVersionadoId)
    {
        var resultado = await _fluxoExecutor.ExecutarFluxoVersionado(fluxoVersionadoId);
        return Ok(resultado);
    }
}
