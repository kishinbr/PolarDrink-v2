using Microsoft.EntityFrameworkCore.Storage;
using PolarDrinks.Data;

namespace PolarDrinks.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

        public void BeginTransaction()
        {
            _transaction = _db.Database.BeginTransaction();
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public void Commit()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }
    }
}