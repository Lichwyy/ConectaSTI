using ConectaSTI.Dominio.Entidades.Logs;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoLogFluxo : ServicoCrud<LogFluxo>
{
    public ServicoLogFluxo(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}
