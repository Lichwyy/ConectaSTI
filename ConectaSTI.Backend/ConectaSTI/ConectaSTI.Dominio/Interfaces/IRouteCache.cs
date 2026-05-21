using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Dominio.Interfaces;

public interface IRouteCache
{
    IReadOnlyList<Rota> GetOrSet(VerboHttp metodo, Func<IReadOnlyList<Rota>> factory);
    void Remove(VerboHttp metodo);
    void RemoveAll();
}
