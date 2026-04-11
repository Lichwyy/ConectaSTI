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
    private readonly IRepositorioConsulta _repositorioConsulta;
    
    public EndPointController(ServicoEndPoint servico, IMapper mapper, IRequestExecutor request, IRepositorioConsulta repositorioConsulta) : base(servico, mapper)
    {
        _request = request;
        _repositorioConsulta = repositorioConsulta;
    }

    [HttpPost("/penis/{NoId}")]
    public IActionResult Testar(int NoId)
    {
        var no = _repositorioConsulta.Consulta<No>(no => no.Id == NoId).FirstOrDefault();

        return Ok(_request.EnviarRequisicao(no));
    }
}