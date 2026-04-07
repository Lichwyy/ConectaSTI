using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Integracao : EntidadeBase
{
    [Obrigar, Uri]
    public string Url { get; set; }
    public string Token { get; set; }
}