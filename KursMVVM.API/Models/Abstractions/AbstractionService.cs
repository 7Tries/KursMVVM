namespace KursMVVM.API.Models.Abstractions;

/// <summary>
/// Базовый абстрактный класс-сервис, предоставляющий безопасное выполнение операций.
/// </summary>
public abstract class AbstractionService
{
    /// <summary>
    /// Выполняет действие и возвращает true при успехе, false при ошибке.
    /// </summary>
    public bool DoAction(Action action)
    {
        try
        {
            action.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
