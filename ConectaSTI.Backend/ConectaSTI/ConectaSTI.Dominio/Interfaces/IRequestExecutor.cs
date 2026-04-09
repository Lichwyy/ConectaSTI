using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Dominio.Interfaces;

public interface IRequestExecutor
{
    public RespostaHttp<object> EnviarRequisicao(No nozinho);
}