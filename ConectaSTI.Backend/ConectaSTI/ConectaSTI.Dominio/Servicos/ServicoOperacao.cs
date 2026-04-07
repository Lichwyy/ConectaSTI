using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;
using ConectaSTI.Dominio.ObjetosValor;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoOperacao : ServicoCrud<Operacao>
{
    private IRepositorioConsulta _consulta;
    public ServicoOperacao(IRepositorioSessao repositorio) : base(repositorio)
    {
        _consulta = Repositorio.GetRepositorioConsulta();
    }

    public override bool Validacoes(Operacao entidade)
    {
        Mensagens.Clear();
        if (entidade == null)
        {
            Mensagens.Add("Operação é obrigatória.", true);
            return false;
        }

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
            Mensagens.Add("No não existe.", true);
            return;
        }

        ValidarTipoNoParaOrdem(entidade, no);
    }

    private void ValidarTipoNoParaOrdem(Operacao entidade, No no)
    {
        // Operação de ordem 1 não pode ser do tipo PegarStorage
        // pois o storage ainda não foi populado neste ponto do fluxo
        if (entidade.Ordem == 1 && !no.PodeSerPrimeiraOperacao())
        {
            Mensagens.Add("A primeira operação (ordem 1) não pode ser do tipo PegarStorage, pois o storage ainda não possui dados neste ponto do fluxo.", true);
        }
    }

    private void ValidarOrdemUnica(Operacao entidade)
    {
        if (Consulta(x => x.FluxoId == entidade.FluxoId && x.Ordem == entidade.Ordem && x.Id != entidade.Id).Any())
            Mensagens.Add("Já existe uma operação com a mesma ordem para este fluxo.", true);
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
}