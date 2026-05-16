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
}