using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoOperacao : ServicoCrud<Operacao>
{
    private IRepositorioConsulta _consulta;

    public ServicoOperacao(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio, currentUserContext)
    {
        _consulta = Repositorio.GetRepositorioConsulta();
    }

    public override bool Validacoes(Operacao entidade)
    {
        Mensagens.Clear();
        if (entidade == null)
        {
            Mensagens.Add("OperaÃ§Ã£o Ã© obrigatÃ³ria.", true);
            return false;
        }

        SincronizarFluxo(entidade);

        ValidarOrdem(entidade);
        ValidarNoExiste(entidade);
        ValidarOrdemUnica(entidade);
        ValidarPoliticaRepeticao(entidade);

        return !Mensagens.HasErro();
    }

    private void ValidarOrdem(Operacao entidade)
    {
        if (entidade.Ordem <= 0)
            Mensagens.Add("Ordem deve ser maior que zero.", true);
    }

    private void ValidarNoExiste(Operacao entidade)
    {
        No no = _consulta.Retorna<No>(entidade.NoId);
        if (no == null)
        {
            Mensagens.Add("No nÃ£o existe.", true);
            return;
        }

        ValidarTipoNoParaOrdem(entidade, no);
    }

    private void ValidarTipoNoParaOrdem(Operacao entidade, No no)
    {
        // OperaÃ§Ã£o de ordem 1 nÃ£o pode ser do tipo PegarStorage
        // pois o storage ainda nÃ£o foi populado neste ponto do fluxo
        if (entidade.Ordem == 1 && !no.PodeSerPrimeiraOperacao())
        {
            Mensagens.Add("A primeira operaÃ§Ã£o (ordem 1) nÃ£o pode ser do tipo PegarStorage, pois o storage ainda nÃ£o possui dados neste ponto do fluxo.", true);
        }
    }

    private void ValidarOrdemUnica(Operacao entidade)
    {
        if (Consulta(x => x.FluxoId == entidade.FluxoId && x.Ordem == entidade.Ordem && x.Id != entidade.Id).Any())
            Mensagens.Add("JÃ¡ existe uma operaÃ§Ã£o com a mesma ordem para este fluxo.", true);
    }

    private void ValidarPoliticaRepeticao(Operacao entidade)
    {
        try
        {
            entidade.ValidarPoliticaRepeticao();
        }
        catch (InvalidOperationException ex)
        {
            Mensagens.Add(ex.Message, true);
        }
    }

    private static void SincronizarFluxo(Operacao entidade)
    {
        if (entidade.Fluxo != null && entidade.FluxoId <= 0)
        {
            entidade.FluxoId = entidade.Fluxo.Id;
        }

        if (entidade.Fluxo == null && entidade.FluxoId > 0)
        {
            entidade.Fluxo = new Fluxo { Id = entidade.FluxoId };
        }
    }
}
