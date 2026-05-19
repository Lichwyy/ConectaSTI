using AutoMapper;
using FGB.Dominio.Entidades;
using FGB.Servicos;

namespace FGB.Api.Controllers
{
    public class LogPropriedadeController : ConsultaControllerBase<LogPropriedade, LogPropriedade>
    {
        public LogPropriedadeController(ServicoConsulta<LogPropriedade> servico, IMapper mapper) : base(servico, mapper)
        {
        }
    }
}
