using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers;

public class OperacaoController : CrudControllerBase<Operacao, Operacao>
{
    public OperacaoController(ServicoCrud<Operacao> servico, IMapper mapper) : base(servico, mapper)
    {
    }
}