using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoIntegracao : ServicoCrud<Integracao>
{
    public ServicoIntegracao(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}