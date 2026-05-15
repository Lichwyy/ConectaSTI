using ConectaSTI.Dominio.ObjetosValor;

namespace ConectaSTI.Dominio.DTOs;

public class OperacaoDTO
{
    public int Ordem { get; set; }
    public long NoId { get; set; }
    public bool Repetir { get; set; } = false;
    public bool UsarDadosAnterior { get; set; } = false;
    public TipoErro Erro { get; set; }
    public int MaximoRepeticao { get; set; }
    public BackoffType BackoffType { get; set; } = BackoffType.Immediate;
    public int BackoffDelay { get; set; } = 0;
    public double BackoffMultiplier { get; set; } = 1.0;
    public int Timeout { get; set; } = 30000;
}