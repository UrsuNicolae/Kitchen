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

                return Ok(PrepareOrder());
            }
            return BadRequest("Model state is invalid");
        }

        #region helpers

        private async Task StoreOrder(Order order)
        {
            StaticContext.Orders.Add(order);
        }

        private SendOrderDto PrepareOrder()
        {
            while (StaticContext.Orders.Any())
            {
                if (StaticContext.Cooks.Any(c => c.IsAvailable))
                {
                    var order = StaticContext.Orders.ToList().ElementAt(0);
                    Console.WriteLine($"--> Start preparing order {order.Id}");
                    var foodComplexity = 0;
                    foreach (var food in order.Foods)
                    {
                        StaticContext.FoodsToPrepare.Add(new OrderFood
                        {
                            Food = food,
                            OrderId = order.Id
                        });
                        if (food.Complexity > foodComplexity) foodComplexity = food.Complexity;
                    }

                    while(StaticContext.FoodsToPrepare.Any(f => f.OrderId == order.Id))
                    {
                        foreach(var food in StaticContext.FoodsToPrepare)
                        {
                            if (food.IsPreparing)
                            {
                                StaticContext.FoodsToPrepare.Remove(food);
                            }

                            Cook cook = null;
                            while(cook == null)
                            {
                                cook = StaticContext.Cooks.FirstOrDefault(c => c.IsAvailable && c.Rank >= food.Food.Complexity );
                            }
                            food.IsPreparing = true;
                            cook.StartPreparing(food);
                        }
                    }

                    Console.WriteLine($"--> Finish preparing order: {order.Id}");
                    return new SendOrderDto
                    {
                        CreatedAt = order.CreatedAt,
                        Foods = order.Foods,
                        Id = order.Id,
                        MaxWaitTime = order.MaxWaitTime,
                        PreparedIn = DateTime.UtcNow.Subtract(order.ReceivedAt),
                        Priority = order.Priority,
                        ReceivedAt = order.ReceivedAt
                    };
                    //var cook = StaticContext.Cooks.FirstOrDefault(c => c.IsAvailable && c.Rank >= foodComplexity);
                    //if (cook != null)
                    //{
                    //    return cook.StartPreparing(order);
                    //}
                };
            }

            return new SendOrderDto();
        }
        #endregion
    }
}
