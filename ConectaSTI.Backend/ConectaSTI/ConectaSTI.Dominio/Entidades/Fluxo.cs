using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Fluxo : EntidadeBase
{
    [Obrigar]
    public string Nome { get; set; }
    public IList<Operacao> Operacoes { get; set; }
}