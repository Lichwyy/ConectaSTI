using AutoMapper;
using ConectaSTI.Dominio.Entidades.Logs;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers
{
    public class LogFluxoController : ConsultaControllerBase<LogFluxo, LogFluxo>
    {
        public LogFluxoController(ServicoLogFluxo servico, IMapper mapper) : base(servico, mapper)
        {
        }
    }
}
