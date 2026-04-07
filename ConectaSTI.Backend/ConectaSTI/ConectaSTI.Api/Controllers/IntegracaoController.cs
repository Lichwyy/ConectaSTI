using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;

namespace ConectaSTI.Api.Controllers;

public class IntegracaoController : CrudControllerBase<Integracao, Integracao>
{
    public IntegracaoController(ServicoIntegracao servico, IMapper mapper) : base(servico, mapper)
    {
    }
}