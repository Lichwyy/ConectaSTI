using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Dominio.Interfaces
{
    public interface IFluxoExecutor
    {
        Task<RespostaHttp<object>> Executar(long fluxoId);
        Task<RespostaHttp<object>> ExecutarFluxoVersionado(long fluxoVersionadoId);
    }
}
