using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;
using System.Threading;

namespace ConectaSTI.Dominio.Interfaces;

public interface IRequestExecutor
{
    public RespostaHttp<object> EnviarRequisicao(No nozinho, object dadoAnterior = null, CancellationToken cancellationToken = default);
}
