using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;

namespace ConectaSTI.Api.Controllers;

public class FuncaoController : CrudControllerBase<Funcao, Funcao>
{
    public FuncaoController(ServicoFuncao servico, IMapper mapper) : base(servico, mapper)
    {
    }
}