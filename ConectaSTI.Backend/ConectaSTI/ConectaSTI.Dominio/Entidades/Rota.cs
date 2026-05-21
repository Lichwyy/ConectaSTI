using FGB.Dominio.Atributos;
using FGB.Dominio.ObjetoValor;
using FGB.Entidades;

namespace ConectaSTI.Dominio.Entidades
{
    public class Rota : EntidadeBase
    {
        [Obrigar]
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string SenhaAcesso { get; set; } = null; // senha que tem que estar no header da requisição para acessar a rota, se for nula ou vazia, não é necessário senha
        public bool UsarFluxoMaisAtual { get; set; } = false;
        public bool RateLimit { get; set; } = false;
        public int RateLimitRequests { get; set; } = 0;
        public int RateLimitInterval { get; set; } = 60;
        public bool Idempotencia { get; set; } = false;
        public string JsonSchemaResp { get; set; }
        public string JsonSchemaReq { get; set; }
        [Obrigar]
        public long PipelineVersaoId { get; set; }
        public string PipelineVersao { get; set; } //somente para mostrar na tabela do front
        [Obrigar]
        public string Caminho { get; set; }
        [Obrigar]
        public VerboHttp Metodo { get; set; }
    }
}
