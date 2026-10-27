using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using KursMVVM.API.Models;
using KursMVVM.API.Models.Data;
using KursMVVM.API.Models.Services;

namespace KursMVVM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly ILogger<EmployeesController> _logger;
    private readonly EmployeeService _service;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "EMPLOYEES_ALL";

    public EmployeesController(KursContext db, ILogger<EmployeesController> logger, IMemoryCache memoryCache)
    {
        _service = new EmployeeService(db);
        _logger = logger;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Получить список сотрудников (с постраничной выборкой).
    /// </summary>
    [HttpGet]
    [Produces(typeof(Employee[]))]
    public async Task<ActionResult<IEnumerable<Employee>>> GetPage([FromQuery] int? page)
    {
        if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<Employee>? cached))
        {
            cached = await _service.GetPage(page);
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(30),
                Size = cached.Count()
            };
            _memoryCache.Set(CacheKey, cached, cacheOptions);
        }

        var stats = _memoryCache.GetCurrentStatistics();
        _logger.LogInformation("Cache hits: {Hits}, Size: {Size}", stats?.TotalHits, stats?.CurrentEstimatedSize);

        return Ok(cached ?? Enumerable.Empty<Employee>());
    }

    /// <summary>
    /// Получить всех сотрудников без пагинации.
    /// </summary>
    [HttpGet("all")]
    [Produces(typeof(Employee[]))]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    /// <summary>
    /// Получить сотрудника по ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Employee>> GetById(int id)
    {
        var result = await _service.Get(id);
        return result is null
            ? NotFound(new { message = $"Сотрудник с ID={id} не найден." })
            : Ok(result);
    }

    /// <summary>
    /// Создать нового сотрудника.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Employee>> Create([FromBody] Employee employee)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Create(employee))
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        return BadRequest(new { message = "Ошибка при создании сотрудника." });
    }

    /// <summary>
    /// Обновить данные сотрудника.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Employee>> Update(int id, [FromBody] Employee employee)
    {
        if (employee.Id != id)
            return BadRequest(new { message = "ID в теле запроса не совпадает с ID в маршруте." });

        _memoryCache.Remove(CacheKey);
        if (_service.Update(id, employee))
            return Ok(employee);
        return NotFound(new { message = $"Сотрудник с ID={id} не найден." });
    }

    /// <summary>
    /// Удалить сотрудника.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Delete(id))
            return NoContent();
        return NotFound(new { message = $"Сотрудник с ID={id} не найден." });
    }
}
