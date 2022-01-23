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
        private readonly DbContextOptions<AppDbContext> _contextOptions;

        public ServeController(AppDbContext context, DbContextOptions<AppDbContext> contextOptions)
        {
            _context = context;
            _contextOptions = contextOptions;
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

                    var foodComplexity = 0;
                    foreach (var food in order.Foods)
                    {
                        if (food.Complexity > foodComplexity) foodComplexity = food.Complexity;
                    }
                    
                    var cook = await _context.Cooks.FirstOrDefaultAsync(c => c.IsAvailable && c.Rank >= foodComplexity);
                    while (cook != null)
                    {
                        cook.IsAvailable = false;
                        var proficiency = cook.Proficiency;
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"--> Start preparing order {order.Id}");
                        Parallel.ForEach(currentPreparedOrder.Foods, async food =>
                        {
                            using (var ctx = new AppDbContext(_contextOptions))
                            {

                                if (food.CookingApparatus == CookingApparatuses.None)
                                {
                                    Console.WriteLine($"--> Start preparing food: {food.Name}");
                                    if (proficiency == 0)
                                    {
                                        Thread.Sleep(food.PreparationTime * 100); //preparing food
                                        proficiency = cook.Proficiency;
                                    }
                                    else
                                    {
                                        proficiency--;
                                    }

                                    Console.WriteLine($"-->Finish preparing food: {food.Name}");
                                }
                                else
                                {
                                    while (await ctx.CookingApparatuses.FirstOrDefaultAsync(c =>
                                        c.TypeOfApparatus == food.CookingApparatus) == null)
                                    {
                                        Console.WriteLine(
                                            $"--> Waiting for {Enum.GetName(food.CookingApparatus.GetType(), food.CookingApparatus)}");
                                    }

                                    var cookingApparatus =
                                        await ctx.CookingApparatuses.FirstOrDefaultAsync(c =>
                                            c.TypeOfApparatus == food.CookingApparatus);
                                    cookingApparatus.IsFree = false;
                                    await ctx.SaveChangesAsync();
                                    Console.WriteLine($"--> Start preparing food: {food.Name}");
                                    if (proficiency == 0)
                                    {
                                        Thread.Sleep(food.PreparationTime * 100); //preparing food
                                        proficiency = cook.Proficiency;
                                    }
                                    else
                                    {
                                        proficiency--;
                                    }

                                    Thread.Sleep(food.PreparationTime * 100); //preparing food
                                    Console.WriteLine($"-->Finish preparing food: {food.Name}");
                                    cookingApparatus.IsFree = true;
                                    await ctx.SaveChangesAsync();
                                }
                            }

                        });
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
                    }
                };
            }

            return new SendOrderDto();
        }
        #endregion
    }
}
