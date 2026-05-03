using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.ObjetoValor;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Dominio.Interfaces
{
    public interface IStorageExecutor
    {
        public RespostaHttp<object> Salvar(No no);
        public RespostaHttp<object> Pegar(string chave);
    }
}
