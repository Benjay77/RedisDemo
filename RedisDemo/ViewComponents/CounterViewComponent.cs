using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisDemo.ViewComponents
{
    public class CounterViewComponent:ViewComponent
    {
        private readonly IConnectionMultiplexer _redis;

        private readonly IDatabase _db;

        public CounterViewComponent(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var controller = RouteData.Values["controller"] as string;
            var action = RouteData.Values["action"] as string;
            if (!string.IsNullOrEmpty(controller)&& !string.IsNullOrEmpty(action))
            {
                var pageId = $"{controller}--{action}";
                await _db.StringIncrementAsync(pageId);

                var count = await _db.StringGetAsync(pageId);
                return View("Default", pageId+":" + count);
            }
            throw new Exception("Cannot get pageId");
        }
    }
}
