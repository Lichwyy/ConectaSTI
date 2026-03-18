using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoOperacao : ServicoCrud<Operacao>
{
    public ServicoOperacao(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}