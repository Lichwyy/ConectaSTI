using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Funcao : EntidadeBase
{
    public string Nome { get; set; }
    public string CorpoDaFuncao { get; set; }
    public string Parametro { get; set; }
}