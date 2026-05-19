using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades.Logs;

public class LogFluxo : EntidadeBase
{
    public long FluxoId { get; set; }
    public int Versao { get; set; }
}