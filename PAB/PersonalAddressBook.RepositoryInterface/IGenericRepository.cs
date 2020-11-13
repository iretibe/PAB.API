using System;
using System.Threading.Tasks;

namespace PAB.RepositoryInterface
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task Create(TEntity entity);
        void Update(TEntity entity);
        Task Delete(TEntity entity);
        Task<TEntity> GetById(Guid id);
        Task<bool> CodeExists(Guid id);
        Task<bool> Save();
    }
}
