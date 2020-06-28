using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedisDemo.Models;
using StackExchange.Redis;

namespace RedisDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConnectionMultiplexer _redis;

        private readonly IDatabase _db;

        private readonly IDistributedCache _distributedCache;

        public HomeController(ILogger<HomeController> logger, IConnectionMultiplexer redis,IDistributedCache distributedCache)
        {
            _logger = logger;
            _redis = redis;
            _db = _redis.GetDatabase();
            _distributedCache = distributedCache;
        }

        public IActionResult Index()
        {
            //_db.StringSet("fullName", "Benjay Shaw");
            //var name = _db.StringGet("fullName");
            var value = _distributedCache.Get("name-key");
            if (value == null)
            {
                var obj = new Dictionary<string, string>
                {
                    ["FirstName"] = "Benjay",
                    ["LastName"] = "Shaw"
                };
                var str = JsonConvert.SerializeObject(obj);
                byte[] encoded = Encoding.UTF8.GetBytes(str);

                var options = new  DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));
                _distributedCache.Set("name-key", encoded,options);

                return View(obj);
            }
            else
            {
                var str = Encoding.UTF8.GetString(value);
                var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
                return View(obj);
            }           
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
