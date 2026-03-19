using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Fluxo : EntidadeBase
{
    public string Nome { get; set; }
    public List<Operacao> Operacoes { get; set; }
}