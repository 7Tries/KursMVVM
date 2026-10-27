namespace KursMVVM.API.Models.Abstractions;

/// <summary>
/// Общий интерфейс сервиса для работы с сущностями.
/// T — тип модели, K — тип первичного ключа.
/// </summary>
public interface ICommonService<T, K>
{
    bool Create(T model);
    bool Update(K id, T model);
    bool Delete(K id);
    Task<T?> Get(K id);
    Task<IEnumerable<T>> GetAll();
    Task<IEnumerable<T>> GetPage(int? page);
}
