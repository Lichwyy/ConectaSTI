using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades.Logs;

public class LogFluxo : EntidadeBase
{
    public long? FluxoId { get; set; }
    public int Versao { get; set; }
    public long? FluxoVersionadoId { get; set; }
    public string Nome { get; set; }
    public long? LogFluxoPaiId { get; set; }
    public DateTime? IniciadoEm { get; set; }
    public DateTime? FinalizadoEm { get; set; }
    public int DuracaoMs { get; set; }
    public int QuantidadeOperacoes { get; set; }
    public int OperacoesExecutadas { get; set; }
    public int OperacoesComSucesso { get; set; }
    public int OperacoesComFalha { get; set; }
    public int StatusHttp { get; set; }
    public bool Sucesso { get; set; }
}
