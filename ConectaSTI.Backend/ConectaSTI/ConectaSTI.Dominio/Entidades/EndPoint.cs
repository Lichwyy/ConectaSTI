using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class EndPoint : EntidadeBase
{
    public string Recurso { get; set; }
    public long UrlId { get; set; }
}