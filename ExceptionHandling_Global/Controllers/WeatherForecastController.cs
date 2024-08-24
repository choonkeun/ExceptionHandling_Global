using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace ExceptionHandling_Global.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMemoryCache _cache;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        [SwaggerOperation(
            OperationId = "GetWeatherForecast", 
            Summary = "This method use 'cache' of 10 seconds", 
            Description = "This method returns same results in the time period for fast response.")]
        public IEnumerable<WeatherForecast> Get()
        {
            var lst = new List<WeatherForecast>();

            if (!_cache.TryGetValue("WFCache", out lst))
            {
                lst = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToList();

                //storing in cache
                _cache.Set("WFCache",
                    lst,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
                );
            }

            return lst;
        }
    }
}
