using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using FGB.Dominio.Interfaces.Utilitarios;
using FGB.IRepositorios;
using Microsoft.AspNetCore.Mvc;

namespace ConectaSTI.Api.Controllers;

public class EndPointController : CrudControllerBase<EndPoint, EndPoint>
{
    private readonly IRequestExecutor _request;
    private readonly IFunctionExecutor _executor;
    private readonly IRepositorioConsulta _repositorioConsulta;
    
    public EndPointController(ServicoEndPoint servico, IMapper mapper, IRequestExecutor request, IRepositorioConsulta repositorioConsulta, IFunctionExecutor executor) : base(servico, mapper)
    {
        _request = request;
        _executor = executor;
        _repositorioConsulta = repositorioConsulta;
    }

    [HttpPost("/testerequest/{NoId}")]
    public IActionResult Testar(int NoId)
    {
        var no = _repositorioConsulta.Consulta<No>(no => no.Id == NoId).FirstOrDefault();

        return Ok(_request.EnviarRequisicao(no));
    }
    
    [HttpPost("/testefunction/{FuncaoId}")]
    public IActionResult TestarFunction(int FuncaoId, [FromBody] object dadoAnterior)
    {
        var funcao = _repositorioConsulta.Consulta<Funcao>(funcao => funcao.Id == FuncaoId).FirstOrDefault();
        
        return Ok(_executor.Executar(funcao, dadoAnterior));
    }
}