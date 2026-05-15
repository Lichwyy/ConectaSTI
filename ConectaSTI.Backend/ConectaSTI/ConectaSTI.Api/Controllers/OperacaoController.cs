using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;

namespace ConectaSTI.Api.Controllers;

public class OperacaoController : ConsultaControllerBase<Operacao, Operacao>
{
    public OperacaoController(ServicoOperacao servico, IMapper mapper) : base(servico, mapper)
    {
    }
}