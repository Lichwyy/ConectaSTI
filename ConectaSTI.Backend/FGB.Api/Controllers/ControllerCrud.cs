using AutoMapper;
using FGB.Entidades;
using FGB.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGB.Api.Controllers
{
    public abstract class CrudControllerBase<T, TDto> : ConsultaControllerBase<T, TDto>
        where T : EntidadeBase
    {
        protected readonly ServicoCrud<T> _servico;

        protected CrudControllerBase(ServicoCrud<T> servico, IMapper mapper)
            : base(servico, mapper)
        {
            _servico = servico;
        }

        [Authorize(Policy = "admin")]
        [HttpPost]
        public virtual IActionResult Post([FromBody] T entidade)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_servico.Inclui(entidade))
                return Ok(new { mensagem = $"{typeof(T).Name} cadastrado com sucesso.", entidade });

            return UnprocessableEntity(_servico.Mensagens);
        }


        [HttpPut("{id:long}")]
        [Authorize(Policy = "admin")]
        public virtual IActionResult Put(long id, [FromBody] T entidade)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            entidade.Id = id;

            var atualizado = _servico.Merge(entidade);
            if (atualizado != null)
                return Ok(new { mensagem = $"{typeof(T).Name} atualizado com sucesso.", entidade });

            return UnprocessableEntity(_servico.Mensagens);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = "admin")]
        public virtual IActionResult Delete(long id)
        {
            var removido = _servico.Exclui(id);
            if (removido != null)
                return Ok(new { mensagem = $"{typeof(T).Name} excluído com sucesso." });

            return UnprocessableEntity(_servico.Mensagens);
        }
    }
}
