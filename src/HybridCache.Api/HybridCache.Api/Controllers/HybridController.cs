using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace Hybrid.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HybridController : ControllerBase
{
    private readonly HybridCache _hybridCache;

    public HybridController(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    [HttpGet("get-using-cache/{name}/{age}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsingCache(
        [FromRoute] string name,
        [FromRoute] long age,
        CancellationToken token)
    {
        var result = await _hybridCache.GetOrCreateAsync(
            key: $"{name}-{age}",
            async cancel => await GetDataFromTheSourceAsync(name, age, cancel),
            cancellationToken: token
        );

        return Ok(result);
    }

    [HttpGet("get-using-cache/v2/{name}/{age}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsingCacheV2(
        [FromRoute] string name,
        [FromRoute] long age,
        CancellationToken token)
    {
        var tags = new List<string> { "tag1", "tag2", "tag3" };

        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(1)
        };

        var result = await _hybridCache.GetOrCreateAsync(
            key: $"{name}-{age}",
            factory: async cancel => await GetDataFromTheSourceAsync(name, age, cancel),
            options: entryOptions,
            tags: tags,
            cancellationToken: token
        );

        return Ok(result);
    }

    private static async Task<string> GetDataFromTheSourceAsync(
        string name,
        long age,
        CancellationToken token)
    {
        await Task.Delay(5000, token);

        return $"canalDEPLOY-{name}-{age}";
    }
}