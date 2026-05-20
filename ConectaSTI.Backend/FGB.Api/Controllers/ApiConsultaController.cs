using AutoMapper;
using AutoMapper.QueryableExtensions;
using FGB.Entidades;
using FGB.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace FGB.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public abstract class ConsultaControllerBase<T, TDto> : ControllerBase
        where T : EntidadeBase
    {
        protected readonly ServicoConsulta<T> _servicoConsulta;
        protected readonly IMapper _mapper;

        protected ConsultaControllerBase(ServicoConsulta<T> servico, IMapper mapper)
        {
            _servicoConsulta = servico;
            _mapper = mapper;
        }

        [HttpGet]
        [EnableQuery]
        //[Authorize(Policy = "admin")]
        public virtual IActionResult GetOData()
        {
            if (typeof(T) != typeof(TDto))
            {
                var listaDto = _servicoConsulta.Consulta()
                    .ProjectTo<TDto>(_mapper.ConfigurationProvider);

                return Ok(listaDto);
            }

            return Ok(_servicoConsulta.Consulta());
        }

        [HttpGet("{id:long}")]
        //[Authorize(Policy = "admin")]
        public virtual IActionResult GetById(long id)
        {
            var entity = _servicoConsulta.Retorna(id);
            if (entity == null)
                return NotFound(new { mensagem = $"{typeof(T).Name} não encontrado." });

            if (typeof(T) != typeof(TDto))
            {
                var dto = _mapper.Map<TDto>(entity);
                return Ok(dto);
            }

            return Ok(entity);
        }
    }
}
