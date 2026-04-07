using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;

namespace ConectaSTI.Api.Controllers;

public class FluxoController : CrudControllerBase<Fluxo, Fluxo>
{
    public FluxoController(ServicoFluxo servico, IMapper mapper) : base(servico, mapper)
    {
    }
}