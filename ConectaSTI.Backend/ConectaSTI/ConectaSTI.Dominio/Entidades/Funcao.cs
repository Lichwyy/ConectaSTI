using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Funcao : EntidadeBase
{
    [Obrigar]
    public string Nome { get; set; }
    [Obrigar]
    
    public string CorpoDaFuncao { get; set; }
    public string Parametro { get; set; }
}