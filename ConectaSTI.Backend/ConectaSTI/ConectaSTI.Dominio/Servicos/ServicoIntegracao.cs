using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoIntegracao : ServicoCrud<Integracao>
{
    public ServicoIntegracao(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio, currentUserContext)
    {
    }
}
