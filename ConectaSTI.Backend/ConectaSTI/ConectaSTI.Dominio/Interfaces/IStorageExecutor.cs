using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConectaSTI.Dominio.Interfaces
{
    public interface IStorageExecutor
    {
        public RespostaHttp<object> Salvar(No no, CancellationToken cancellationToken = default);
        public RespostaHttp<object> Pegar(string chave, CancellationToken cancellationToken = default);
    }
}
