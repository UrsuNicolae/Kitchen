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
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace Kitchen.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ServeController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public ServeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
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



        private async Task<SendOrderDto> PrepareOrder()
        {
            while (StaticContext.Orders.Any())
            {
                if (StaticContext.Cooks.Any(c => c.IsAvailable))
                {
                    StaticContext.orderNr++;
                    var order = StaticContext.Orders.ElementAt(StaticContext.orderNr);
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

                    var orderConfirmation = new OrderDetails
                    {
                        Id = order.Id,
                        WaiterId = order.WaiterId,
                        TableId = order.TableId,
                        Priority = order.Priority,
                        MaxWaitTime = order.MaxWaitTime,
                        PickUpTime = DateTime.UtcNow
                    };



                    foreach (var food in StaticContext.FoodsToPrepare.Where(f => f.OrderId == order.Id))
                    {
                        orderConfirmation.Items.Add(food.Food.Id);
                        orderConfirmation.CookingDetails.Add((food.Food.Id, StaticContext.Cooks.FirstOrDefault(c => c.Rank >= food.Food.Complexity).Id));
                    }

                    var client = new HttpDataClient(_httpClient);
                    client.SendOrder(orderConfirmation);

                    foreach (var food in StaticContext.FoodsToPrepare)
                    {
                        Cook cook = null;
                        while (cook == null)
                        {
                            cook = StaticContext.Cooks.FirstOrDefault(c => c.IsAvailable && c.Rank >= food.Food.Complexity);
                        }

                        food.IsPreparing = true;
                        cook.StartPreparing(food);
                        StaticContext.FoodsToPrepare.Remove(food);
                        Console.WriteLine($"--> Finish preparing order: {order.Id}");
                        if (StaticContext.FoodsToPrepare.All(f => f.OrderId != order.Id))
                        {
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
                        }
                    }



                    return new SendOrderDto();
                };
            }

            return new SendOrderDto();
        }
        #endregion
    }

    public class HttpDataClient
    {
        private readonly HttpClient _httpClient;

        public HttpDataClient(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> SendOrder(OrderDetails order)
        {
            var httpContent = new StringContent(
                JsonConvert.SerializeObject(order, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }),
                Encoding.UTF8,
                "application/json");
            Console.WriteLine($"--> Send orderconfirmation{order.Id}");
            var url = $"https://localhost:3001/api/Serve/Distribution";
            return await _httpClient.PostAsync(url, httpContent);
        }
    }
}
