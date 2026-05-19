using ConectaSTI.Dominio.Entidades.Logs;
using FGB.Dominio.Extensoes;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoLogFluxo : ServicoCrud<LogFluxo>
{
    public ServicoLogFluxo(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio, currentUserContext)
    {
    }

    public override bool Inclui(params LogFluxo[] entidades)  // mão precisamos de log para essa sacanagem
    {
        return ProcessoInclusao(entidades, () => MakeCrudTransaction(repo =>
        {
            foreach (var entidade in entidades)
            {
                entidade.VincularColecoes();
                entidade.CriadoEm = entidade.CriadoEm ?? DateTime.Now;
                entidade.UltimaAlteracao = DateTime.Now;
                repo.Inclui(entidade);
            }
        }));
    }

    public override async Task<bool> IncluiAsync(params LogFluxo[] entidades)
    {
        return await ProcessoInclusaoAsync(entidades, async () => await MakeCrudTransactionAsync(async repo =>
        {
            foreach (var entidade in entidades)
            {
                entidade.VincularColecoes();
                entidade.CriadoEm = entidade.CriadoEm ?? DateTime.Now;
                entidade.UltimaAlteracao = DateTime.Now;
                await repo.IncluiAsync(entidade);
            }
        }));
    }
}
