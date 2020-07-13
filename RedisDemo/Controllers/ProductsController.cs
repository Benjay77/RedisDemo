using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RedisDemo.Data;
using RedisDemo.Models;
using RedisDemo.ViewModels;
using StackExchange.Redis;

namespace RedisDemo.Controllers
{
    public class ProductsController : Controller
    {

        private readonly IConnectionMultiplexer _redis;

        private readonly IDatabase _db;

        private readonly ApplicationDbContext _applicationDbContext;

        public ProductsController(ApplicationDbContext applicationDbContext,IConnectionMultiplexer redis)
        {
            _applicationDbContext = applicationDbContext;
            _redis = redis;
            _db = _redis.GetDatabase();
        }   

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _applicationDbContext.Products.ToListAsync();
            if (products!=null&&products.Count>0)
            {
                var vms = products.Select(x=>new ProductViewModel
                { 
                    Id = x.Id,
                    Name = x.Name
                }).ToList().OrderBy(a=>a.ViewCount);

                RedisKey[] redisKeys = vms.Select(x => (RedisKey)$"Product:{x.Id}:Views").ToArray();
                var viewCounts = await _db.StringGetAsync(redisKeys);

                foreach (var item in vms)
                {
                    var id = item.Id;
                    var key = $"Product:{id}:Views";
                    var index = redisKeys.IndexOf(key);

                    if (index>-1)
                    {
                        item.ViewCount = (int)viewCounts[index];
                    }
                }
                return View("View",vms);
            }
            else
            {
                return View();
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            Product product = await _applicationDbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product!=null)
            {
                await _db.StringIncrementAsync($"Product:{product.Id}:Views");
                product.ViewCount = (int)await _db.StringGetAsync($"Product:{product.Id}:Views");

                var viewKey = "recentViewedProducts";
                var element = $"产品:{product.Name}({ product.Id})次数:{product.ViewCount}";

                await _db.ListLeftPushAsync(viewKey, element);

                return View(product);
            }
            else
            {
                return View();
            }
        }

        public async Task<IActionResult> RecentViewedProducts()
        {
            var list = await _db.ListRangeAsync("recentViewedProducts", 0, 4);
            await _db.ListTrimAsync("recentViewedProducts", 0, 4);

            return View(list);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Products/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Products/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
