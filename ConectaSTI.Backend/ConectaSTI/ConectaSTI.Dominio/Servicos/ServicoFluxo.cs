using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.ObjetosValor;
using FGB.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoFluxo : ServicoCrud<Fluxo>
{
    private ServicoOperacao _servicoOperacao;
    private ServicoNo _servicoNo;
    private IRepositorioConsulta _consulta;
    
    public ServicoFluxo(IRepositorioSessao repositorio) : base(repositorio)
    {
        _consulta = Repositorio.GetRepositorioConsulta();
        _servicoOperacao = new ServicoOperacao(repositorio);
        _servicoNo = new ServicoNo(repositorio);
    }

    public override bool Validacoes(Fluxo entidade)
    {
        if (entidade == null)
        {
            Mensagens.Add("Fluxo é obrigatório.", true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(entidade.Nome))
            Mensagens.Add("Nome do fluxo é obrigatório.", true);

        ValidarOperacoes(entidade);

        return !Mensagens.HasErro();
    }

    private void ValidarOperacoes(Fluxo entidade)
    {
        if (entidade.Operacoes == null || !entidade.Operacoes.Any())
        {
            Mensagens.Add("O fluxo deve conter pelo menos uma operação.", true);
            return;
        }

        ValidarOrdemSequencial(entidade);
        ValidarPrimeiraOperacao(entidade);
        ValidarDependenciasStorage(entidade);

        foreach (var operacao in entidade.Operacoes)
        {
            _servicoOperacao.Validacoes(operacao);
        }

        if (_servicoOperacao.Mensagens.Any(m => m.Erro))
        {
            Mensagens.AddRange(_servicoOperacao.Mensagens);
        }
    }

    private void ValidarOrdemSequencial(Fluxo entidade)
    {
        var ordens = entidade.Operacoes.Select(o => o.Ordem).OrderBy(o => o).ToList();
        
        // Verifica se há ordens duplicadas
        if (ordens.Distinct().Count() != ordens.Count)
            Mensagens.Add("Existem operações com a mesma ordem no fluxo.", true);

        // Verifica se as ordens são sequenciais começando em 1
        for (int i = 0; i < ordens.Count; i++)
        {
            if (ordens[i] != i + 1)
            {
                Mensagens.Add($"A ordem das operações deve ser sequencial começando em 1. Esperado: {i + 1}, Encontrado: {ordens[i]}.", true);
                break;
            }
        }
    }

    private void ValidarPrimeiraOperacao(Fluxo entidade)
    {
        var primeiraOperacao = entidade.Operacoes.FirstOrDefault(o => o.Ordem == 1);
        if (primeiraOperacao == null)
        {
            Mensagens.Add("O fluxo deve ter uma operação de ordem 1.", true);
            return;
        }

        No no = _consulta.Retorna<No>(primeiraOperacao.NoId);
        if (no != null && !no.PodeSerPrimeiraOperacao())
        {
            Mensagens.Add("A primeira operação (ordem 1) não pode ser do tipo PegarStorage, pois o storage ainda não foi populado.", true);
        }
    }

    private void ValidarDependenciasStorage(Fluxo entidade)
    {
        var operacoesOrdenadas = entidade.Operacoes.OrderBy(o => o.Ordem).ToList();
        var chavesSalvas = new HashSet<string>();

        foreach (var operacao in operacoesOrdenadas)
        {
            No no = _consulta.Retorna<No>(operacao.NoId);
            if (no == null) continue;

            if (no.Tipo == TipoNo.SalvarStorage && !string.IsNullOrEmpty(no.ChaveValor))
            {
                chavesSalvas.Add(no.ChaveValor);
            }

            if (no.Tipo == TipoNo.PegarStorage && !string.IsNullOrEmpty(no.ChaveValor))
            {
                if (!chavesSalvas.Contains(no.ChaveValor))
                {
                    Mensagens.Add($"Operação de ordem {operacao.Ordem} tenta acessar a chave '{no.ChaveValor}' do storage, mas esta chave não foi salva em nenhuma operação anterior no fluxo.", true);
                }
            }
        }
    }
}