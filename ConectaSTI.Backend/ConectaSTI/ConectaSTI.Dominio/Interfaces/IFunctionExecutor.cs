using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Dominio.Interfaces;

public interface IFunctionExecutor
{
    public RespostaHttp<object> Executar(Funcao funcao, object dadoAnterior);
}