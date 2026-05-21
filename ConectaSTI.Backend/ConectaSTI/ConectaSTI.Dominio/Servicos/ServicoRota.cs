using ConectaSTI.Dominio.Entidades;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos
{
    public class ServicoRota : ServicoCrud<Rota>
    {
        public ServicoRota(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio, currentUserContext)
        {
        }
        public override bool Validacoes(Rota entidade)
        {
            Mensagens.Clear();
            if (entidade == null)
            {
                Mensagens.Add("Rota é obrigatória.", true);
                return false;
            }
            FluxoVersionado fluxo = Repositorio.GetRepositorioConsulta().Consulta<FluxoVersionado>(f => f.Id == entidade.PipelineVersaoId).FirstOrDefault();
            if (fluxo == null)
            {
                Mensagens.Add("Pipeline versão não encontrada.", true);
                return false;
            }
            entidade.PipelineVersao = fluxo.Nome + "_" + fluxo.Versao.ToString();

            Rota rotaExistente = Consulta(r => r.Caminho == entidade.Caminho && r.Metodo == entidade.Metodo && r.Id != entidade.Id).FirstOrDefault();
            if (rotaExistente != null)
            {
                Mensagens.Add("Já existe uma rota com o mesmo caminho e método.", true);
                return false;
            }

            return !Mensagens.HasErro();
        }
    }
}
