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
}