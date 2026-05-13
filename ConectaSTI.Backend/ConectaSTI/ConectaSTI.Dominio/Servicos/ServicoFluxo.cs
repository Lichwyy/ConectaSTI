using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoFluxo : ServicoCrud<Fluxo>
{
    private ServicoOperacao _servicoOperacao;
    private IRepositorioConsulta _consulta;
    
    public ServicoFluxo(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio, currentUserContext)
    {
        _consulta = Repositorio.GetRepositorioConsulta();
        _servicoOperacao = new ServicoOperacao(repositorio, currentUserContext);
    }

    public override bool Validacoes(Fluxo entidade)
    {
        Mensagens.Clear();
        if (entidade == null)
        {
            Mensagens.Add("Fluxo Ã© obrigatÃ³rio.", true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(entidade.Nome))
            Mensagens.Add("Nome do fluxo Ã© obrigatÃ³rio.", true);

        ValidarOperacoes(entidade);

        return !Mensagens.HasErro();
    }

    private void ValidarOperacoes(Fluxo entidade)
    {
        if (entidade.Operacoes == null || !entidade.Operacoes.Any())
        {
            Mensagens.Add("O fluxo deve conter pelo menos uma operaÃ§Ã£o.", true);
            return;
        }

        ValidarOrdemSequencial(entidade);
        ValidarPrimeiraOperacao(entidade);
        ValidarDependenciasStorage(entidade);

        foreach (var operacao in entidade.Operacoes)
        {
            _servicoOperacao.Validacoes(operacao);
            _servicoOperacao.Mensagens.ForEach(m => Mensagens.Add($"OperaÃ§Ã£o Ordem {operacao.Ordem}: {m.Mensagem}", m.Erro));
        }
    }

    private void ValidarOrdemSequencial(Fluxo entidade)
    {
        var ordens = entidade.Operacoes.Select(o => o.Ordem).OrderBy(o => o).ToList();
        
        // Verifica se hÃ¡ ordens duplicadas
        if (ordens.Distinct().Count() != ordens.Count)
            Mensagens.Add("Existem operaÃ§Ãµes com a mesma ordem no fluxo.", true);

        // Verifica se as ordens sÃ£o sequenciais comeÃ§ando em 1
        for (int i = 0; i < ordens.Count; i++)
        {
            if (ordens[i] != i + 1)
            {
                Mensagens.Add($"A ordem das operaÃ§Ãµes deve ser sequencial comeÃ§ando em 1. Esperado: {i + 1}, Encontrado: {ordens[i]}.", true);
                break;
            }
        }
    }

    private void ValidarPrimeiraOperacao(Fluxo entidade)
    {
        var primeiraOperacao = entidade.Operacoes.FirstOrDefault(o => o.Ordem == 1);
        if (primeiraOperacao == null)
        {
            Mensagens.Add("O fluxo deve ter uma operaÃ§Ã£o de ordem 1.", true);
            return;
        }

        No no = _consulta.Retorna<No>(primeiraOperacao.NoId);
        if (no != null && !no.PodeSerPrimeiraOperacao())
        {
            Mensagens.Add("A primeira operaÃ§Ã£o (ordem 1) nÃ£o pode ser do tipo PegarStorage, pois o storage ainda nÃ£o foi populado.", true);
        }
    }

    private void ValidarDependenciasStorage(Fluxo entidade)
    {
        var operacoesOrdenadas = entidade.Operacoes.OrderBy(o => o.Ordem).ToList();
        var chavesSalvas = new HashSet<string>();

        foreach (var operacao in operacoesOrdenadas)
        {
            No no = _consulta.Retorna<No>(operacao.NoId); // depois pretendo colocar uma interface de cache, no momento deixa assim
            if (no == null) continue;

            if (no.Tipo == TipoNo.SalvarStorage && !string.IsNullOrEmpty(no.ChaveValor))
            {
                chavesSalvas.Add(no.ChaveValor);
            }

            if (no.Tipo == TipoNo.PegarStorage && !string.IsNullOrEmpty(no.ChaveValor))
            {
                if (!chavesSalvas.Contains(no.ChaveValor))
                {
                    Mensagens.Add($"OperaÃ§Ã£o de ordem {operacao.Ordem} tenta acessar a chave '{no.ChaveValor}' do storage, mas esta chave nÃ£o foi salva em nenhuma operaÃ§Ã£o anterior no fluxo.", true);
                }
            }
        }
    }
}
