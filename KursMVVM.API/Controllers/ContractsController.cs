using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using KursMVVM.API.Models;
using KursMVVM.API.Models.Data;
using KursMVVM.API.Models.Services;

namespace KursMVVM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContractsController : ControllerBase
{
    private readonly ILogger<ContractsController> _logger;
    private readonly ContractService _service;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "CONTRACTS_ALL";

    public ContractsController(KursContext db, ILogger<ContractsController> logger, IMemoryCache memoryCache)
    {
        _service = new ContractService(db);
        _logger = logger;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Получить список договоров (с постраничной выборкой).
    /// </summary>
    [HttpGet]
    [Produces(typeof(Contract[]))]
    public async Task<ActionResult<IEnumerable<Contract>>> GetPage([FromQuery] int? page)
    {
        if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<Contract>? cached))
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

        return Ok(cached ?? Enumerable.Empty<Contract>());
    }

    /// <summary>
    /// Получить все договоры без пагинации.
    /// </summary>
    [HttpGet("all")]
    [Produces(typeof(Contract[]))]
    public async Task<ActionResult<IEnumerable<Contract>>> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    /// <summary>
    /// Получить договор по номеру.
    /// </summary>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Contract>> GetById(string id)
    {
        var result = await _service.Get(id);
        return result is null
            ? NotFound(new { message = $"Договор №{id} не найден." })
            : Ok(result);
    }

    /// <summary>
    /// Создать новый договор.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Contract>> Create([FromBody] Contract contract)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Create(contract))
            return CreatedAtAction(nameof(GetById), new { id = contract.ContractNumber }, contract);
        return BadRequest(new { message = "Ошибка при создании договора." });
    }

    /// <summary>
    /// Обновить договор.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Contract>> Update(string id, [FromBody] Contract contract)
    {
        if (contract.ContractNumber != id)
            return BadRequest(new { message = "Номер договора в теле не совпадает с параметром маршрута." });

        _memoryCache.Remove(CacheKey);
        if (_service.Update(id, contract))
            return Ok(contract);
        return NotFound(new { message = $"Договор №{id} не найден." });
    }

    /// <summary>
    /// Удалить договор.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Delete(id))
            return NoContent();
        return NotFound(new { message = $"Договор №{id} не найден." });
    }
}
