using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoFuncao : ServicoCrud<Funcao>
{
    public ServicoFuncao(IRepositorioSessao repositorio) : base(repositorio)
    {
    }
}