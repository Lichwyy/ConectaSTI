using FGB.Entidades;

namespace FGB.Dominio.Entidades
{
    public class LogPropriedade : EntidadeBase
    {
        public LogEntidade LogEntidade { get; set; }
        public long LogEntidadeId { get; set; }
        public string Propriedade { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorAtual { get; set; }
    }
}
