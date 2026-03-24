using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoNo : ServicoCrud<No>
{
    public ServicoNo(IRepositorioSessao repositorio) : base(repositorio)
    {
    }

    public override bool Valida(No entidade)
    {
        if (!base.Valida(entidade))
            return false;

        try
        {
            entidade.ValidarVinculos();
        }
        catch (InvalidOperationException ex)
        {
            Mensagens.Add(ex.Message, true);
        }

        return !Mensagens.HasErro();
    }
}