using ConectaSTI.Dominio.ObjetosValor;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectaSTI.Dominio.Entidades.Logs
{
    public class LogOperacao
    {
        // ID da execucao de fluxo, pois o mesmo fluxo pode rodar várias vezes, esse ID separa cada execução. (Temos que verificar)
        public long ExecucaoId { get; set; }

        // ID da operação para identificar qual operação foi executada e que gerou esse log.
        public long OperacaoId { get; set; }

        // ID do fluxo ao qual essa operação pertence
        public long FluxoId { get; set; }

        // Ordem da operação dentro do fluxo
        public int Ordem { get; set; }

        // Tipo do nó executado (Requisicao, FuncaoJS, Storage, etc.)
        public TipoNo TipoNo { get; set; }

        // Input recebido pela operação
        public string Input { get; set; }

        // Output gerado pela operação
        public string Output { get; set; }

        // Quantas vezes essa operação foi repetida
        public int RepeticoesFeitas { get; set; }

        // Status final da operação (ex: Retryando, Falha, Sucesso)
        public StatusOperacao Status { get; set; }

        // Tipo de erro definido na operação (ex: parar fluxo, continuar, compensar)
        public TipoErro TipoErro { get; set; }

        // Mensagem de erro capturada durante execução
        // ESSENCIAL pra debug (ex: exception.Message)
        public string MensagemErro { get; set; }

        // Stack trace do erro
        public string StackTrace { get; set; }

        // Código HTTP retornado (apenas para operações de requisição) (ex: 200, 404, 500)
        public int? StatusHttp { get; set; }

        // Headers da resposta HTTP (serializados como string/JSON)
        public string HeadersResposta { get; set; }

        // Indica se a operação falhou por timeout (diferencia erro de lógica vs tempo excedido)
        public bool Timeout { get; set; }

        // Momento em que a execução da operação começou
        public DateTime DataInicio { get; set; }

        // Momento em que a execução terminou
        public DateTime DataTermino { get; set; }

        // Duração total da operação em milissegundos
        public double Duracao => (DataTermino - DataInicio).TotalMilliseconds;
    }
}
