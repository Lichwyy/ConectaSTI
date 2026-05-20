using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;

namespace ConectaSTI.Api.Controllers
{
    public class RotaController : CrudControllerBase<Rota, Rota>
    {
        public RotaController(ServicoRota servico, IMapper mapper) : base(servico, mapper)
        {
        }
    }
}
