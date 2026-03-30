using ConectaSTI.Dominio.Entidades;
using FGB.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoFluxo : ServicoCrud<Fluxo>
{
    public ServicoFluxo(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
    override public bool Valida(Fluxo entidade)
    {
        if (entidade.Nome.Length > 100)
            throw new Exception("O nome do fluxo deve conter no máximo 100 caracteres.");

        return true;
    }
}