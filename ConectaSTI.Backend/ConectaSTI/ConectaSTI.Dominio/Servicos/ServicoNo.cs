using ConectaSTI.Dominio.Entidades;
using ConectaSTI.Dominio.ObjetosValor;
using FGB.Dominio.Interfaces.Seguranca;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoNo : ServicoCrud<No>
{
    private IRepositorioConsulta _consulta;

    public ServicoNo(IRepositorioSessao repositorio, ICurrentUserContext currentUserContext) : base(repositorio, currentUserContext)
    {
        _consulta = Repositorio.GetRepositorioConsulta();
    }

    public override bool Validacoes(No entidade)
    {
        Mensagens.Clear();
        if (entidade == null)
        {
            Mensagens.Add("NÃ³ Ã© obrigatÃ³rio.", true);
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
        //Validando Chave Valor
        if (string.IsNullOrEmpty(entidade.ChaveValor))
        {
            Mensagens.Add("ChaveValor Ã© obrigatÃ³ria para nÃ³s do tipo SalvarStorage.", true);
            return;
        }
            
        No storageExistente = Consulta(s => s.ChaveValor == entidade.ChaveValor && s.Id != entidade.Id && s.Tipo == TipoNo.SalvarStorage)
            .FirstOrDefault();
        if (storageExistente != null)
            Mensagens.Add("JÃ¡ existe um nÃ³ com a mesma ChaveValor.", true);

        // Validando TempoMinutoValidade - deve ser um valor positivo
        if (entidade.TempoMinutoValidade <= 0)
            Mensagens.Add("TempoMinutoValidade deve ser um valor positivo.", true);
    }

    private void ValidarPegarStorage(No entidade)
    {
        if (string.IsNullOrEmpty(entidade.ChaveValor))
        {
            Mensagens.Add("ChaveValor Ã© obrigatÃ³ria para nÃ³s do tipo PegarStorage.", true);
            return;
        }

        No salvarStorageExistente = Consulta(s => s.ChaveValor == entidade.ChaveValor && s.Tipo == TipoNo.SalvarStorage).FirstOrDefault();
        if (salvarStorageExistente == null)
            Mensagens.Add($"NÃ£o existe um nÃ³ SalvarStorage com a ChaveValor '{entidade.ChaveValor}'. PegarStorage sÃ³ pode referenciar chaves previamente salvas.", true);
    }

    private void ValidarRequisicao(No entidade)
    {
        if (!entidade.EndPointId.HasValue)
        {
            Mensagens.Add("EndPointId Ã© obrigatÃ³rio para nÃ³s do tipo Requisicao.", true);
            return;
        }

        var endpoint = _consulta.Retorna<EndPoint>(entidade.EndPointId.Value);
        if (endpoint == null)
            Mensagens.Add("EndPoint nÃ£o existe.", true);
    }

    private void ValidarFuncao(No entidade)
    {
        if (!entidade.FuncaoId.HasValue)
        {
            Mensagens.Add("FuncaoId Ã© obrigatÃ³ria para nÃ³s do tipo FuncaoJS.", true);
            return;
        }

        var funcao = _consulta.Retorna<Funcao>(entidade.FuncaoId.Value);
        if (funcao == null)
            Mensagens.Add("FunÃ§Ã£o nÃ£o existe.", true);
    }
}
