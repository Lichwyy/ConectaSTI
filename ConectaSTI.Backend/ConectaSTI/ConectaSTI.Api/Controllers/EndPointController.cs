using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers;

public class EndPointController : CrudControllerBase<EndPoint, EndPoint>
{
    public EndPointController(ServicoCrud<EndPoint> servico, IMapper mapper) : base(servico, mapper)
    {
    }
}