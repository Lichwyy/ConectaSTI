using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Dominio.Interfaces;

public interface IFunctionExecutor
{
    public RespostaHttp<string> Executar(Funcao funcao, object dadoAnterior);
}