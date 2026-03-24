using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.Atributos;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades;

public class No : EntidadeBase
{
    [Obrigar]
    public TipoNo Tipo { get; set; } // Tem que fazer nengue
    public long? FuncaoId { get; set; }
    public long? EndPointId { get; set; }

    public void ValidarVinculos()
    {
        if (Tipo == TipoNo.FuncaoJS)
        {
            if (!FuncaoId.HasValue || EndPointId.HasValue)
                throw new InvalidOperationException("No do tipo FuncaoJS deve ter FuncaoId e nao deve ter EndPointId.");
            return;
        }

        if (Tipo == TipoNo.Requisicao)
        {
            if (!EndPointId.HasValue || FuncaoId.HasValue)
                throw new InvalidOperationException("No do tipo Requisicao deve ter EndPointId e nao deve ter FuncaoId.");
        }
    }
}