using ConectaSTI.Dominio.ObjetosValor;
using System;
using System.Collections.Generic;
using System.Text;
using FGB.Dominio.ObjetoValor;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades.Logs
{
    public class LogOperacao  : EntidadeBase
    {
        public long LogFluxoId { get; set; }
        public long NoId { get; set; }
        public long? FuncaoId { get; set; }
        public long? EndPointId { get; set; }
        
        //Integracao
        public string Nome { get; set; }
        public string Url { get; set; }
        
        // Endpoint
        public string Recurso { get; set; }
        public VerboHttp Verbo { get; set; }
        public string Token { get; set; }
        
        // No
        public TipoNo Tipo { get; set; }
        public string Body { get; set; }
        public string Headers { get; set; }
        public string ChaveValor { get; set; }
        public int TempoMinutoValidade { get; set; } = 0;
        
        // Operacao
        public int Ordem { get; set; }
        public bool Repetir { get; set; } = false;
        public bool UsarDadosAnterior { get; set; } = false;
        public TipoErro Erro { get; set; }
        public int MaximoRepeticao { get; set; }
        public BackoffType BackoffType { get; set; } = BackoffType.Immediate;
        public int BackoffDelay { get; set; } = 0;
        public double BackoffMultiplier { get; set; } = 1.0;
        public int Timeout { get; set; } = 30000;

        // Resultado da execucao
        public DateTime IniciadoEm { get; set; }
        public DateTime FinalizadoEm { get; set; }
        public int DuracaoMs { get; set; }
        public int TentativasRealizadas { get; set; }
        public int AtrasoTotalMs { get; set; }
        public int StatusHttp { get; set; }
        public bool Sucesso { get; set; }
        public string DadoAnterior { get; set; }
        public string Resposta { get; set; }
        public string RespostaBody { get; set; }
        public string MensagensRetorno { get; set; }
        public string MensagemErro { get; set; }
        public string ExceptionTipo { get; set; }
        public string StackTrace { get; set; }
    }
}
