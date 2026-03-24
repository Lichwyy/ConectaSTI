using ConectaSTI.Dominio.Entidades;
using FGB.IRepositorios;
using FGB.Servicos;

namespace ConectaSTI.Dominio.Servicos;

public class ServicoOperacao : ServicoCrud<Operacao>
{
    public ServicoOperacao(IRepositorioSessao repositorio) : base(repositorio)
    {
    }

    public override bool Valida(Operacao entidade)
    {
        if (!base.Valida(entidade))
            return false;

        try
        {
            entidade.ValidarPoliticaRepeticao();
        }
        catch (InvalidOperationException ex)
        {
            Mensagens.Add(ex.Message, true);
        }

        if (entidade.Ordem <= 0)
            Mensagens.Add("Ordem deve ser maior que zero.", true);

        if (Consulta(x => x.FluxoId == entidade.FluxoId && x.Ordem == entidade.Ordem && x.Id != entidade.Id).Any())
            Mensagens.Add("Já existe uma operação com a mesma ordem para este fluxo.", true);

        return !Mensagens.HasErro();
    }
}