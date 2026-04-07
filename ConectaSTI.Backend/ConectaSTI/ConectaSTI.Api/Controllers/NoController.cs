using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;

namespace ConectaSTI.Api.Controllers;

public class NoController : CrudControllerBase<No, No>
{
    public NoController(ServicoNo servico, IMapper mapper) : base(servico, mapper)
    {
    }
}