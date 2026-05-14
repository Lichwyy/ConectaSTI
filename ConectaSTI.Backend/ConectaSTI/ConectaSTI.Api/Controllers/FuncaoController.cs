using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ConectaSTI.Api.Controllers;

public class FuncaoController : CrudControllerBase<Funcao, Funcao>
{
    private readonly ServicoFuncao _servico;

    public FuncaoController(ServicoFuncao servico, IMapper mapper) : base(servico, mapper)
    {
        _servico = servico;
    }

    [HttpPost("/funcao/criar-js")]
    [Consumes("text/plain")]
    [Produces("application/json")]
    public IActionResult CriarJs([FromBody] string corpoDaFuncao, [FromQuery] string nome, [FromQuery] string parametro = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest("O parâmetro 'nome' é obrigatório.");

        if (string.IsNullOrWhiteSpace(corpoDaFuncao))
            return BadRequest("O corpo da função JS não pode ser vazio.");

        var funcao = new Funcao
        {
            Nome = nome,
            Parametro = parametro,
            CorpoDaFuncao = corpoDaFuncao
        };

        if (!_servico.Inclui(funcao))
            return BadRequest(_servico.Mensagens);

        return Ok(funcao);
    }
}