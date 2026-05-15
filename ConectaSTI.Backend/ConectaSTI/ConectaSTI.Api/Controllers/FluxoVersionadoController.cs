using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers;

public class FluxoVersionadoController : ConsultaControllerBase<FluxoVersionado, FluxoVersionado>
{
    public FluxoVersionadoController(ServicoFluxoVersionado servico, IMapper mapper) : base(servico, mapper)
    {
    }
}