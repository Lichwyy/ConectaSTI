using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.ObjetosValor;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ConectaSTI.Api.Controllers;

public class NoController : CrudControllerBase<No, No>
{
    private readonly ServicoNo _servico;

    public NoController(ServicoNo servico, IMapper mapper) : base(servico, mapper)
    {
        _servico = servico;
    }

    [HttpPost("/no/criar-json")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public IActionResult CriarJson([FromBody] CriarNoJsonRequest request)
    {
        if (request == null)
            return BadRequest("Payload inválido.");

        var no = new No
        {
            Tipo = request.Tipo,
            Body = request.Body.ValueKind == JsonValueKind.Undefined || request.Body.ValueKind == JsonValueKind.Null
                ? null
                : request.Body.GetRawText(),
            EndPointId = request.EndPointId,
            FuncaoId = request.FuncaoId,
            ChaveValor = request.ChaveValor,
            TempoMinutoValidade = request.TempoMinutoValidade
        };

        if (!_servico.Inclui(no))
            return BadRequest(_servico.Mensagens);

        return Ok(no);
    }
}

public class CriarNoJsonRequest
{
    public TipoNo Tipo { get; set; }
    public JsonElement Body { get; set; }
    public long? EndPointId { get; set; }
    public long? FuncaoId { get; set; }
    public string ChaveValor { get; set; }
    public int TempoMinutoValidade { get; set; }
}