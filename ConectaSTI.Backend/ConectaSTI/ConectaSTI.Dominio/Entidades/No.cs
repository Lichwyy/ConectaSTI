using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class No : EntidadeBase
{
    [Obrigar]
    public string Tipo { get; set; } // Tem que fazer nengue
    public long? FuncaoId { get; set; }
    public long? EndPointId { get; set; }
}