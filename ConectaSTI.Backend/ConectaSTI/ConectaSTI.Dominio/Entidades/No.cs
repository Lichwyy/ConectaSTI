using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class No : EntidadeBase
{
    [Obrigar]
    public TipoNo Tipo { get; set; }
    public string Body { get; set; }
    public string Headers { get; set; }
    public long? FuncaoId { get; set; }
    public long? EndPointId { get; set; }
    public string ChaveValor { get; set; }
    public DateTime DataValidade { get; set; }

    public void ValidarVinculos()
    {
        if (Tipo == TipoNo.FuncaoJS)
        {
            if (!FuncaoId.HasValue || EndPointId.HasValue || !string.IsNullOrWhiteSpace(ChaveValor))
                throw new InvalidOperationException("No do tipo FuncaoJS deve ter Funcao e nao deve ter EndPoint ou chave.");
        }

        if (Tipo == TipoNo.Requisicao)
        {
            if (!EndPointId.HasValue || FuncaoId.HasValue || !string.IsNullOrWhiteSpace(ChaveValor))
                throw new InvalidOperationException("No do tipo Requisicao deve ter EndPoint e nao deve ter Funcao ou Chave.");
        }

        if (Tipo == TipoNo.SalvarStorage)
        {
            if (string.IsNullOrWhiteSpace(ChaveValor))
                throw new InvalidOperationException("No do tipo SalvarStorage deve ter ChaveValor definida.");
            if (FuncaoId.HasValue || EndPointId.HasValue)
                throw new InvalidOperationException("No do tipo SalvarStorage nao deve ter Funcao ou EndPoint.");
        }

        if (Tipo == TipoNo.PegarStorage)
        {
            if (string.IsNullOrWhiteSpace(ChaveValor))
                throw new InvalidOperationException("No do tipo PegarStorage deve ter ChaveValor definida.");
            if (FuncaoId.HasValue || EndPointId.HasValue)
                throw new InvalidOperationException("No do tipo PegarStorage nao deve ter Funcao ou EndPoint.");
        }
    }

    public bool PodeSerPrimeiraOperacao()
    {
        return Tipo != TipoNo.PegarStorage;
    }
}