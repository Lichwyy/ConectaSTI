using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers;

public class IntegracaoController : CrudControllerBase<Integracao, Integracao>
{
    public IntegracaoController(ServicoCrud<Integracao> servico, IMapper mapper) : base(servico, mapper)
    {
    }
}