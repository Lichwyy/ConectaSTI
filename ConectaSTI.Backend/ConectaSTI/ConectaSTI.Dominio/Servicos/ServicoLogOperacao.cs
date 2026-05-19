using ConectaSTI.Dominio.Entidades.Logs;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoLogOperacao : ServicoCrud<LogOperacao>
{
    public ServicoLogOperacao(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}
