using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Rotas.Interfaces
{
    public interface IRoutingService
    {
        Rota GetRoute(VerboHttp metodo, string caminho, out Dictionary<string, string> parametros);
    }
}
