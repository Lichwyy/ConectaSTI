using ConectaSTI.Dominio.Entidades;

namespace ConectaSTI.Dominio.Interfaces;

public interface IFunctionExecutor
{
    public string Executar(Funcao funcao, object dadoAnterior);
}