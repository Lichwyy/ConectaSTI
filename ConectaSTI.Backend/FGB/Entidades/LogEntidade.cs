using FGB.Entidades;

namespace FGB.Dominio.Entidades
{
    public class LogEntidade : EntidadeBase
    {
        public string NomeEntidade { get; set; }
        public long IdEntidade { get; set; }
        public string TipoOperacao { get; set; }
        public DateTime DataOperacao { get; set; }
        public string Usuario { get; set; }
        public string IdUsuario { get; set; }
    }
}
