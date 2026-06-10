using ConectaSTI.Dominio.DTOs;
using FGB.Dominio.ObjetoValor;

namespace ConectaSTI.Dominio.Interfaces
{
    public interface IFluxoExecutor
    {
        Task<RespostaHttp<object>> Executar(long fluxoId, EntradaFluxoDTO entrada = null);
        Task<RespostaHttp<object>> ExecutarFluxoVersionado(long fluxoVersionadoId, EntradaFluxoDTO entrada = null);
    }
}
