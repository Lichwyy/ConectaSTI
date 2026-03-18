using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers;

public class FluxoController : CrudControllerBase<Fluxo, Fluxo>
{
    public FluxoController(ServicoCrud<Fluxo> servico, IMapper mapper) : base(servico, mapper)
    {
    }
}