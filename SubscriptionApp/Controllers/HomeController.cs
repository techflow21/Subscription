using MassTransit;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using SubscriptionApp.Models;
using System.Diagnostics;

namespace SubscriptionApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IBus _bus;

        public HomeController(ILogger<HomeController> logger, IConnectionMultiplexer redis, IBus bus)
        {
            _logger = logger;
            _redis = redis;
            _bus = bus;
        }

        public async Task<IActionResult> Index()
        {
            // Example of using Redis
            var db = _redis.GetDatabase();
            var cacheKey = "homepage_data";
            var cachedData = await db.StringGetAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedData))
            {
                cachedData = $"Data loaded at {DateTime.Now}";
                await db.StringSetAsync(cacheKey, cachedData, TimeSpan.FromMinutes(1));
                ViewBag.Message = "Data loaded from fresh source and cached!";
            }
            else
            {
                ViewBag.Message = "Data loaded from Redis cache.";
            }

            ViewBag.CachedData = cachedData;
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
