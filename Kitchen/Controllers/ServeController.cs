using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Kitchen.Data;
using Kitchen.DTOs;
using Kitchen.Models;
using Microsoft.EntityFrameworkCore;

namespace Kitchen.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ServeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Order(Order order)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine($"--> Order {order.Id} received at {DateTime.UtcNow}.");
                order.ReceivedAt = DateTime.UtcNow;
                await StoreOrder(order);

                return Ok(await PrepareOrder(order));
            }
            return BadRequest("Model state is invalid");
        }

        #region helpers

        private async Task StoreOrder(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        private async Task<SendOrderDto> PrepareOrder(Order order)
        {
            while (_context.Orders.Any())
            {
                if (_context.Cooks.Any(c => c.IsAvailable))
                {
                    var currentPreparedOrder = _context.Orders.Include(o => o.Foods).ToList().OrderByDescending(o =>
                        o.Foods.Count(f => f.CookingApparatus == CookingApparatuses.None)).ElementAt(0);
                    
                    var cook = _context.Cooks.FirstOrDefault(c => c.IsAvailable);
                    cook.IsAvailable = false;
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"--> Start preparing order {order.Id}");
                    foreach (var food in currentPreparedOrder.Foods)
                    {
                        if (food.CookingApparatus == CookingApparatuses.None)
                        {
                            Console.WriteLine($"--> Start preparing food: {food.Id}");
                            Thread.Sleep(food.PreparationTime * 100);//preparing food
                            Console.WriteLine($"-->Finish preparing food: {food.Id}");
                        }
                        else
                        {
                            while (await _context.CookingApparatuses.FirstOrDefaultAsync(c => c.TypeOfApparatus == food.CookingApparatus) == null)
                            {
                                Console.WriteLine($"--> Waiting for {Enum.GetName(food.CookingApparatus.GetType(), food.CookingApparatus)}");
                            }

                            var cookingApparatus =
                                _context.CookingApparatuses.FirstOrDefaultAsync(c =>
                                    c.TypeOfApparatus == food.CookingApparatus).Result;
                            cookingApparatus.IsFree = false;
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"--> Start preparing food: {food.Id}");
                            Thread.Sleep(food.PreparationTime * 100);//preparing food
                            Console.WriteLine($"-->Finish preparing food: {food.Id}");
                            cookingApparatus.IsFree = true;
                            await _context.SaveChangesAsync();
                        }
                    }
                    Console.WriteLine($"--> Finish preparing order: {order.Id}");
                    cook.IsAvailable = true;
                    await _context.SaveChangesAsync();
                    var orderToReturn = new SendOrderDto
                    {
                        CreatedAt = order.CreatedAt,
                        Foods = order.Foods,
                        Id = order.Id,
                        MaxWaitTime = order.MaxWaitTime,
                        PreparedIn = DateTime.UtcNow.Subtract(order.ReceivedAt),
                        Priority = order.Priority,
                        ReceivedAt = order.ReceivedAt
                    };

                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    return orderToReturn;
                };
            }

            return new SendOrderDto();
        }
        #endregion
    }
}
