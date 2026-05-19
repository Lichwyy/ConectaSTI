using FGB.Dominio.ObjetoValor;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Dominio.Interfaces
{
    public interface IFluxoExecutor
    {
        public Task<RespostaHttp<object>> Executar(long fluxoId);
    }
}
