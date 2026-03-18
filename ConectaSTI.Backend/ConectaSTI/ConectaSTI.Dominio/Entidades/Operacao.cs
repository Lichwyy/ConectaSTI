using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Operacao : EntidadeBase
{
    public string Body { get; set; }
    public string Header { get; set; }
    [Obrigar]
    public string Ordem { get; set; }
    [Obrigar]
    public long NoId { get; set; }
    [Obrigar]
    public long FluxoId { get; set; }
    public bool Repetir { get; set; } = false;
    [Obrigar]
    public string Erro { get; set; } // Tem que fazer nengue
    public int MaximoRepeticao { get; set; }
}