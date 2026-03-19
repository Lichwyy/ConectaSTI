using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers;

public class NoController : CrudControllerBase<No, No>
{
    public NoController(ServicoCrud<No> servico, IMapper mapper) : base(servico, mapper)
    {
    }
}