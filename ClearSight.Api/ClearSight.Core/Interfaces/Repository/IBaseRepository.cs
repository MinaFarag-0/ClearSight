using System.Linq.Expressions;

namespace ClearSight.Core.Interfaces.Repository;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task Update(T entity);
    Task Delete(T entity);
    Task<bool> AnyAsync();
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> criteria);
    Task<T> GetWithIncludesAsync(Expression<Func<T, bool>> criteria, params Expression<Func<T, object>>[] includes);
    Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllWithIncludesAsync(int skip, int take, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllWithIncludesAsync(Expression<Func<T, bool>> criteria, int skip, int take, params Expression<Func<T, object>>[] includes);
}