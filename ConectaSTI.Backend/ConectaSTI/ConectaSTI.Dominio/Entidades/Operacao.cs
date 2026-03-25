using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class Operacao : EntidadeBase
{
    public string Body { get; set; }
    public string Header { get; set; }
    [Obrigar]
    public int Ordem { get; set; }
    [Obrigar]
    public long NoId { get; set; }
    [Obrigar]
    public long FluxoId { get; set; }
    public bool Repetir { get; set; } = false;
    [Obrigar]
    public TipoErro Erro { get; set; } // Tem que fazer nengue
    public int MaximoRepeticao { get; set; }

    public void ValidarPoliticaRepeticao()
    {
        if (!Repetir && MaximoRepeticao > 0)
            throw new InvalidOperationException("Quando Repetir = false, MaximoRepeticao deve ser 0.");

        if (Repetir && MaximoRepeticao <= 0)
            throw new InvalidOperationException("Quando Repetir = true, MaximoRepeticao deve ser maior que 0.");
    }
}