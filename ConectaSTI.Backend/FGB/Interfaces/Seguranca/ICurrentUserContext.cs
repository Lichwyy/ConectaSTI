namespace FGB.Dominio.Interfaces.Seguranca
{
    public interface ICurrentUserContext
    {
        bool IsAuthenticated { get; }
        string UserId { get; }
        string UserName { get; }
    }
}
