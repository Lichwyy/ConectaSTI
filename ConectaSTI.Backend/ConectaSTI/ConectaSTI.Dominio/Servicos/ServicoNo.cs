using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoNo : ServicoCrud<No>
{
    public ServicoNo(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}