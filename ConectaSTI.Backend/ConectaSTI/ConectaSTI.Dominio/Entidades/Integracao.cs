using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Integracao : EntidadeBase
{
    public string Url { get; set; }
    public string Token { get; set; }
}