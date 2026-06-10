using AutoMapper;
using ConectaSTI.Dominio.DTOs;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
    public async Task<IActionResult> ExecutarFluxoVersionado(
        long fluxoVersionadoId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] EntradaFluxoDTO entrada = null)
    {
        var resultado = await _fluxoExecutor.ExecutarFluxoVersionado(fluxoVersionadoId, entrada);
        return Ok(resultado);
    }
}
