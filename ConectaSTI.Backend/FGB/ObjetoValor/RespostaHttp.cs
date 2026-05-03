
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FGB.Servicos;

namespace FGB.Dominio.ObjetoValor
{
    public class RespostaHttp<T>
    {
        public RespostaHttp()
        {
            Retorno = new List<MensagemRetorno>();
        }

        public int Status { get; set; }
        public AcceptProxy Accept { get; set; } = AcceptProxy.Json;
        public T Resposta { get; set; }
        public string RespostaBody { get; set; }
        public TimeSpan? TempoResposta { get; set; }
        public List<MensagemRetorno> Retorno { get; set; }
        public bool Sucesso
        {
            get
            {
                return Status >= 200
                    && Status <= 299
                    && Retorno != null
                    && !Retorno.Any(m => m.Erro);
            }
        }
    }
}
