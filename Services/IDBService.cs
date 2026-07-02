using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursMVVM.Services;

public interface IDBService<T>
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
}
