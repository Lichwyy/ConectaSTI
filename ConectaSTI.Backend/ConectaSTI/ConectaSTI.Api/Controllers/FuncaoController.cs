using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers;

public class FuncaoController : CrudControllerBase<Funcao, Funcao>
{
    public FuncaoController(ServicoCrud<Funcao> servico, IMapper mapper) : base(servico, mapper)
    {
    }
}