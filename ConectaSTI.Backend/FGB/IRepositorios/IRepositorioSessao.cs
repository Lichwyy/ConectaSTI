using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGB.Entidades;

namespace FGB.IRepositorios
{
    public interface IRepositorioSessao : IDisposable
    {
        IRepositorioConsulta GetRepositorioConsulta();

        IRepositorio GetRepositorio();

        T RetornaComLock<T>(long id) where T : EntidadeBase;

        IDisposable IniciaTransacao();

        void CommitaTransacao();

        void RollBackTransacao();

        Task CommitaTransacaoAsync();

        Task RollBackTransacaoAsync();
    }
}
