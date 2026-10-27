using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using KursMVVM.API.Models;
using KursMVVM.API.Models.Data;
using KursMVVM.API.Models.Services;

namespace KursMVVM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SuppliesController : ControllerBase
{
    private readonly ILogger<SuppliesController> _logger;
    private readonly SupplyService _service;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "SUPPLIES_ALL";

    public SuppliesController(KursContext db, ILogger<SuppliesController> logger, IMemoryCache memoryCache)
    {
        _service = new SupplyService(db);
        _logger = logger;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Получить список поставок (с постраничной выборкой).
    /// </summary>
    [HttpGet]
    [Produces(typeof(Supply[]))]
    public async Task<ActionResult<IEnumerable<Supply>>> GetPage([FromQuery] int? page)
    {
        if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<Supply>? cached))
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

        return Ok(cached ?? Enumerable.Empty<Supply>());
    }

    /// <summary>
    /// Получить все поставки без пагинации.
    /// </summary>
    [HttpGet("all")]
    [Produces(typeof(Supply[]))]
    public async Task<ActionResult<IEnumerable<Supply>>> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    /// <summary>
    /// Получить поставку по номеру договора.
    /// </summary>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Supply>> GetById(string id)
    {
        var result = await _service.Get(id);
        return result is null
            ? NotFound(new { message = $"Поставка по договору №{id} не найдена." })
            : Ok(result);
    }

    /// <summary>
    /// Создать новую поставку.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Supply>> Create([FromBody] Supply supply)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Create(supply))
            return CreatedAtAction(nameof(GetById), new { id = supply.ContractNumber }, supply);
        return BadRequest(new { message = "Ошибка при создании поставки." });
    }

    /// <summary>
    /// Обновить поставку.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Supply>> Update(string id, [FromBody] Supply supply)
    {
        if (supply.ContractNumber != id)
            return BadRequest(new { message = "Номер договора в теле не совпадает с параметром маршрута." });

        _memoryCache.Remove(CacheKey);
        if (_service.Update(id, supply))
            return Ok(supply);
        return NotFound(new { message = $"Поставка по договору №{id} не найдена." });
    }

    /// <summary>
    /// Удалить поставку.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Delete(id))
            return NoContent();
        return NotFound(new { message = $"Поставка по договору №{id} не найдена." });
    }
}
