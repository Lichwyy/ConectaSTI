using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Operacao : EntidadeBase
{
    public string Body { get; set; }
    public string Header { get; set; }
    public string Ordem { get; set; }
    public long NoId { get; set; }
    public long FluxoId { get; set; }
}