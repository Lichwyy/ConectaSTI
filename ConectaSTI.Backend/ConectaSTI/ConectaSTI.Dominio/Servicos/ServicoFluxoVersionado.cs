using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoFluxoVersionado : ServicoCrud<FluxoVersionado>
{
    public ServicoFluxoVersionado(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}