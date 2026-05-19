using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos
{
    public class ServicoStorage : ServicoCrud<Storage>
    {
        public ServicoStorage(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio, currentUserContext)
        {
        }
    }
}
