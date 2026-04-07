namespace FGB.IRepositorios
{
    public interface IMigracao
    {
        void UpdateDatabase(string migrationsFolder);
    }
}
