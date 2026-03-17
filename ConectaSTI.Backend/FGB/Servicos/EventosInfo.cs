using FGB.Entidades;

namespace FGB.Servicos
{
    // Delegates para eventos
    public delegate void MergeHandler<T>(MergeInfo<T> info) where T : EntidadeBase;
    public delegate void ExcluiHandler<T>(ExcluiInfo<T> info) where T : EntidadeBase;
    public delegate void IncluiHandler<T>(IncluiInfo<T> info) where T : EntidadeBase;

    // Classes de informação para eventos
    public class MergeInfo<T> where T : EntidadeBase
    {
        public T EntidadeAntiga { get; set; }
        public T EntidadeNova { get; set; }

        public MergeInfo(T entidadeAntiga, T entidadeNova)
        {
            EntidadeAntiga = entidadeAntiga;
            EntidadeNova = entidadeNova;
        }
    }

    public class ExcluiInfo<T> where T : EntidadeBase
    {
        public T Entidade { get; set; }

        public ExcluiInfo(T entidade)
        {
            Entidade = entidade;
        }
    }

    public class IncluiInfo<T> where T : EntidadeBase
    {
        public T Entidade { get; set; }

        public IncluiInfo(T entidade)
        {
            Entidade = entidade;
        }
    }
}
