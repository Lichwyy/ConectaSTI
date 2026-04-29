using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Integracao : EntidadeBase
{
    [Obrigar]
    public string Nome { get; set; }
    [Obrigar, Uri]
    public string Url { get; set; }
    public string Token { get; set; }
    public string Descricao { get; set; }
}