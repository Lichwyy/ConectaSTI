using ConectaSTI.Dominio.DTOs;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class FluxoVersionado : EntidadeBase
{
    public long FluxoId { get; set; }
    public string Nome { get; set; }
    public int Versao { get; set; }
    public string Payload { get; set; }
}