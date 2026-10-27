using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using KursMVVM.API.Models;
using KursMVVM.API.Models.Data;
using KursMVVM.API.Models.Services;

namespace KursMVVM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrganizationsController : ControllerBase
{
    private readonly ILogger<OrganizationsController> _logger;
    private readonly OrganizationService _service;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "ORGANIZATIONS_ALL";

    public OrganizationsController(KursContext db, ILogger<OrganizationsController> logger, IMemoryCache memoryCache)
    {
        _service = new OrganizationService(db);
        _logger = logger;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Получить список организаций (с постраничной выборкой).
    /// </summary>
    [HttpGet]
    [Produces(typeof(Organization[]))]
    public async Task<ActionResult<IEnumerable<Organization>>> GetPage([FromQuery] int? page)
    {
        if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<Organization>? cached))
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

        return Ok(cached ?? Enumerable.Empty<Organization>());
    }

    /// <summary>
    /// Получить все организации без пагинации.
    /// </summary>
    [HttpGet("all")]
    [Produces(typeof(Organization[]))]
    public async Task<ActionResult<IEnumerable<Organization>>> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    /// <summary>
    /// Получить организацию по ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Organization>> GetById(int id)
    {
        var result = await _service.Get(id);
        return result is null
            ? NotFound(new { message = $"Организация с ID={id} не найдена." })
            : Ok(result);
    }

    /// <summary>
    /// Создать новую организацию.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Organization>> Create([FromBody] Organization organization)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Create(organization))
            return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
        return BadRequest(new { message = "Ошибка при создании организации." });
    }

    /// <summary>
    /// Обновить данные организации.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Organization>> Update(int id, [FromBody] Organization organization)
    {
        if (organization.Id != id)
            return BadRequest(new { message = "ID в теле запроса не совпадает с ID в маршруте." });

        _memoryCache.Remove(CacheKey);
        if (_service.Update(id, organization))
            return Ok(organization);
        return NotFound(new { message = $"Организация с ID={id} не найдена." });
    }

    /// <summary>
    /// Удалить организацию.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        _memoryCache.Remove(CacheKey);
        if (_service.Delete(id))
            return NoContent();
        return NotFound(new { message = $"Организация с ID={id} не найдена." });
    }
}
