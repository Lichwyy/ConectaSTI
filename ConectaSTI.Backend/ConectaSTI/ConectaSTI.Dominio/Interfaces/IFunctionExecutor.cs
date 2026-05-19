using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;
using System.Threading;

namespace ConectaSTI.Dominio.Interfaces;

public interface IFunctionExecutor
{
    public RespostaHttp<object> Executar(Funcao funcao, object dadoAnterior, CancellationToken cancellationToken = default);
}