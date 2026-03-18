using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoEndPoint : ServicoCrud<EndPoint>
{
    public ServicoEndPoint(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}