using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.ObjetosValor;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoNo : ServicoCrud<No>
{
    private IRepositorioConsulta _consulta;
    public ServicoNo(IRepositorioSessao repositorio) : base(repositorio)
    {
        _consulta = Repositorio.GetRepositorioConsulta();
    }

    public override bool Validacoes(No entidade)
    {
        if (entidade == null)
        {
            Mensagens.Add("Nó é obrigatório.", true);
            return false;
        }

        ValidarVinculosPorTipo(entidade);

        switch (entidade.Tipo)
        {
            case TipoNo.Requisicao:
                ValidarRequisicao(entidade);
                break;
            case TipoNo.FuncaoJS:
                ValidarFuncao(entidade);
                break;
            case TipoNo.SalvarStorage:
                ValidarSalvarStorage(entidade);
                break;
            case TipoNo.PegarStorage:
                ValidarPegarStorage(entidade);
                break;
        }

        return !Mensagens.HasErro();
    }

    private void ValidarVinculosPorTipo(No entidade)
    {
        try
        {
            entidade.ValidarVinculos();
        }
        catch (InvalidOperationException ex)
        {
            Mensagens.Add(ex.Message, true);
        }
    }

    private void ValidarSalvarStorage(No entidade)
    {
        if (string.IsNullOrEmpty(entidade.ChaveValor))
            Mensagens.Add("ChaveValor é obrigatória para nós do tipo SalvarStorage.", true);
        
        No storageExistente = Consulta(s => s.ChaveValor == entidade.ChaveValor && s.Id != entidade.Id).FirstOrDefault();
        if (storageExistente != null)
            Mensagens.Add("Já existe um nó com a mesma ChaveValor.", true);
    }

    private void ValidarPegarStorage(No entidade)
    {
        if (string.IsNullOrEmpty(entidade.ChaveValor))
            Mensagens.Add("ChaveValor é obrigatória para nós do tipo PegarStorage.", true);

        No salvarStorageExistente = Consulta(s => s.ChaveValor == entidade.ChaveValor && s.Tipo == TipoNo.SalvarStorage).FirstOrDefault();
        if (salvarStorageExistente == null)
            Mensagens.Add($"Não existe um nó SalvarStorage com a ChaveValor '{entidade.ChaveValor}'. PegarStorage só pode referenciar chaves previamente salvas.", true);
    }

    private void ValidarRequisicao(No entidade)
    {
        if (!entidade.EndPointId.HasValue)
            Mensagens.Add("EndPointId é obrigatório para nós do tipo Requisicao.", true);


        var endpoint = _consulta.Retorna<EndPoint>(entidade.EndPointId.Value);
        if (endpoint == null)
            Mensagens.Add("EndPoint não existe.", true);
    }

    private void ValidarFuncao(No entidade)
    {
        if (!entidade.FuncaoId.HasValue)
            Mensagens.Add("FuncaoId é obrigatória para nós do tipo FuncaoJS.", true);

        var funcao = _consulta.Retorna<Funcao>(entidade.FuncaoId.Value);
        if (funcao == null)
            Mensagens.Add("Função não existe.", true);
    }
}