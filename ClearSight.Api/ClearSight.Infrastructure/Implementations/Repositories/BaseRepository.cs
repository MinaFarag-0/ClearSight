using ClearSight.Core.Interfaces.Repository;
using ClearSight.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace ClearSight.Infrastructure.Implementations.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<T?> GetByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            return entities;
        }

        public async Task Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public async Task Update(T entity)
        {
            _context.Update(entity);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<T>().CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> criteria)
        {
            return await _context.Set<T>().CountAsync(criteria);
        }
        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(int skip, int take, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Skip(skip).Take(take);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.ToList();
        }
        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(Expression<Func<T, bool>> criteria, int skip, int take, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().Where(criteria).OrderDescending().Skip(skip).Take(take);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.ToList();
        }
        public async Task<T?> GetWithIncludesAsync(Expression<Func<T, bool>> criteria, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var incluse in includes)
                    query = query.Include(incluse);

            return await query.FirstOrDefaultAsync(criteria);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }
        public async Task<bool> AnyAsync()
        {
            return await _context.Set<T>().AnyAsync();
        }
        public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var incluse in includes)
                    query = query.Include(incluse);

            return await query.SingleOrDefaultAsync(criteria);
        }

    }
}