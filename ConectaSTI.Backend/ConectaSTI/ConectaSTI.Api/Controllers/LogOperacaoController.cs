using AutoMapper;
using ConectaSTI.Dominio.Entidades.Logs;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using FGB.Servicos;

namespace ConectaSTI.Api.Controllers
{
    public class LogOperacaoController : ConsultaControllerBase<LogOperacao, LogOperacao>
    {
        public LogOperacaoController(ServicoLogOperacao servico, IMapper mapper) : base(servico, mapper)
        {
        }
    }
}
