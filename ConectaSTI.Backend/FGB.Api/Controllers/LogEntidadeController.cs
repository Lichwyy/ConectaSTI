using AutoMapper;
using FGB.Dominio.Entidades;
using FGB.Servicos;

namespace FGB.Api.Controllers
{
    public class LogEntidadeController : ConsultaControllerBase<LogEntidade, LogEntidade>
    {
        public LogEntidadeController(ServicoConsulta<LogEntidade> servico, IMapper mapper) : base(servico, mapper)
        {
        }
    }
}
