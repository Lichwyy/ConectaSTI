using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.Atributos;
using FGB.Entidades;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ConectaSTI.Dominio.Entidades;

public class Operacao : EntidadeBase
{
    [Obrigar]
    public int Ordem { get; set; }
    [Obrigar(typeof(No))]
    public long NoId { get; set; }
    public long FluxoId { get; set; }
    [JsonIgnore]
    public Fluxo Fluxo { get; set; }
    public bool Repetir { get; set; } = false;
    [Obrigar]
    public TipoErro Erro { get; set; } // Tem que fazer nengue
    public int MaximoRepeticao { get; set; }

    [Range(0, 10, ErrorMessage = "MaxRetries deve estar entre 0 e 10")]
    public int MaxRetries { get; set; } = 0;
    [Obrigar]
    public BackoffType BackoffType { get; set; } = BackoffType.Immediate;
    [Range(0, int.MaxValue, ErrorMessage = "BackoffDelay deve ser um inteiro positivo")]
    public int BackoffDelay { get; set; } = 0;
    [Range(1.0, 5.0, ErrorMessage = "BackoffMultiplier deve estar entre 1.0 e 5.0")]
    public double BackoffMultiplier { get; set; } = 1.0;

    [Range(1000, 300000, ErrorMessage = "Timeout deve estar entre 1000ms e 300000ms")]
    public int Timeout { get; set; } = 30000;

    public void ValidarPoliticaRepeticao()
    {
        if (!Repetir && MaximoRepeticao > 0)
            throw new InvalidOperationException("Quando Repetir = false, MaximoRepeticao deve ser 0.");

        if (Repetir && MaximoRepeticao <= 0)
            throw new InvalidOperationException("Quando Repetir = true, MaximoRepeticao deve ser maior que 0.");
    }
}