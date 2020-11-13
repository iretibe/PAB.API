using PAB.Entity;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PAB.RepositoryInterface;

namespace PAB.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        private PABContext _context;

        public GenericRepository(PABContext context)
        {
            _context = context;
        }


        public async Task<bool> CodeExists(Guid id)
        {
            return await _context.Set<TEntity>().AnyAsync(o => o.pkId == id);
        }

        public async Task Create(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TEntity> GetById(Guid id)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.pkId == id);
        }

        public async Task<bool> Save()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void Update(TEntity entity)
        {

        }

    }
}
