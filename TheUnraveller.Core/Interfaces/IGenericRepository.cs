using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheUnraveller.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Add(T entity);
    void Update(T entity);
    Task UpdateAsync(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
    Task<T?> GetByIdAsync(int id);
}
