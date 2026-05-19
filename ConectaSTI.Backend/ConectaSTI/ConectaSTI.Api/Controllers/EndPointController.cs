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
    public EndPointController(ServicoEndPoint servico, IMapper mapper) : base(servico, mapper)
    {
    }
}