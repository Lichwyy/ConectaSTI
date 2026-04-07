using FGB.Dominio.Atributos;
using FGB.Dominio.ObjetoValor;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class EndPoint : EntidadeBase
{
    [Obrigar]
    public string Recurso { get; set; }
    [Obrigar]
    public long IntegracaoId { get; set; }
    [Obrigar]
    public VerboHttp Verbo { get; set; }
}