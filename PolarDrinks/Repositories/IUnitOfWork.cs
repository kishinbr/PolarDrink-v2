namespace PolarDrinks.Repositories
{
    /// <summary>
    /// Controla transações de banco de dados sem expor detalhes do Entity Framework
    /// para as camadas de cima (Service).
    /// </summary>
    public interface IUnitOfWork
    {
        void BeginTransaction();
        void SaveChanges();
        void Commit();
        void Rollback();
    }
}