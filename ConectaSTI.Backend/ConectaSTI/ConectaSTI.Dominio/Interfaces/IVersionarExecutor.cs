using ConectaSTI.Dominio.Entidades;

namespace ConectaSTI.Dominio.Interfaces;

public interface IVersionarExecutor
{
    public FluxoVersionado Execute(long fluxoId);
}