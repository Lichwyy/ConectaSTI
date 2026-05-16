using AutoMapper;
using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.Interfaces;
using ConectaSTI.Dominio.Servicos;
using FGB.Api.Controllers;
using FGB.IRepositorios;
using Microsoft.AspNetCore.Mvc;

namespace ConectaSTI.Api.Controllers;

public class EndPointController : CrudControllerBase<EndPoint, EndPoint>
{
    private readonly IRequestExecutor _request;
    private readonly IFunctionExecutor _executor;
    private readonly IStorageExecutor _storageExecutor;
    private readonly IFluxoExecutor _fluxoExecutor;
    private readonly IRepositorioConsulta _repositorioConsulta;
    
    public EndPointController(ServicoEndPoint servico, IMapper mapper, IRequestExecutor request, IRepositorioConsulta repositorioConsulta, IFunctionExecutor executor, IStorageExecutor storageExecutor, IFluxoExecutor fluxoExecutor) : base(servico, mapper)
    {
    }
}