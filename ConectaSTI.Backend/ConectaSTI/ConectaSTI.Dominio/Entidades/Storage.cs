using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Storage : EntidadeBase
{
    public string Chave { get; set; }
    public string Valor { get; set; }
    public DateTime Validade { get; set; }
}