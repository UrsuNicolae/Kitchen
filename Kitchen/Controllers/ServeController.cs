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

        public ServeController()
        {
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
            StaticContext.Orders.Add(order);
        }

        private async Task<SendOrderDto> PrepareOrder(Order order)
        {
            while (StaticContext.Orders.Any())
            {
                if (StaticContext.Cooks.Any(c => c.IsAvailable))
                {
                    var currentPreparedOrder = StaticContext.Orders.ToList().OrderByDescending(o =>
                        o.Foods.Count(f => f.CookingApparatus == CookingApparatuses.None)).ElementAt(0);

                    var foodComplexity = 0;
                    foreach (var food in order.Foods)
                    {
                        if (food.Complexity > foodComplexity) foodComplexity = food.Complexity;
                    }
                    
                    var cook = StaticContext.Cooks.FirstOrDefault(c => c.IsAvailable && c.Rank >= foodComplexity);
                    while (cook != null)
                    {
                        cook.IsAvailable = false;
                        var proficiency = cook.Proficiency;
                        Console.WriteLine($"--> Start preparing order {order.Id}");
                        Parallel.ForEach(currentPreparedOrder.Foods,  food =>
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
                                    while (StaticContext.CookingApparatuses.FirstOrDefault(c =>
                                        c.TypeOfApparatus == food.CookingApparatus) == null)
                                    {
                                        Console.WriteLine(
                                            $"--> Waiting for {Enum.GetName(food.CookingApparatus.GetType(), food.CookingApparatus)}");
                                    }

                                    var cookingApparatus = StaticContext.CookingApparatuses.FirstOrDefault(c =>
                                            c.TypeOfApparatus == food.CookingApparatus);
                                    cookingApparatus.IsFree = false;
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
                                }
                        });
                        Console.WriteLine($"--> Finish preparing order: {order.Id}");
                        cook.IsAvailable = true;
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
                        return orderToReturn;
                    }
                };
            }

            return new SendOrderDto();
        }
        #endregion
    }
}
