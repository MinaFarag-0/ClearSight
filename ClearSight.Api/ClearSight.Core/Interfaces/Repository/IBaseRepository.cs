using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
    Task<IEnumerable<T>> GetAllWithIncludesAsync(Expression<Func<T, bool>> criteria, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
    Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int skip,int take );
    Task<IEnumerable<T>> FindAllAsync(int skip,int take );
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? take, int? skip, Expression<Func<T, object>> orderBy = null, string orderByDirection = "ASC");
    Task<IEnumerable<T>> GetAllWithIncludesAsync(int skip, int take, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllWithIncludesAsync(Expression<Func<T, bool>> criteria, int skip, int take, params Expression<Func<T, object>>[] includes);
}