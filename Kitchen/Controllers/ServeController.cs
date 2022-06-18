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
        public async Task Order(OrderWithIds order)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine($"--> Order {order.Order.Id} received at {DateTime.UtcNow}.");
                order.Order.ReceivedAt = DateTime.UtcNow;
                await StoreOrder(order);

                PrepareOrder();
            }
        }

        #region helpers

        private async Task StoreOrder(OrderWithIds order)
        {
            StaticContext.Orders.Add(order);
        }



        private async Task PrepareOrder()
        {
            while (StaticContext.Orders.Any())
            {
                if (StaticContext.Cooks.Any(c => c.IsAvailable))
                {
                    StaticContext.orderNr++;
                    var order = StaticContext.Orders.ElementAt(StaticContext.orderNr);
                    Console.WriteLine($"--> Start preparing order {order.Order.Id}");
                    var foodComplexity = 0;
                    foreach (var food in order.Order.Foods)
                    {
                        StaticContext.FoodsToPrepare.Add(new OrderFood
                        {
                            Food = food,
                            OrderId = order.Order.Id
                        });
                        if (food.Complexity > foodComplexity) foodComplexity = food.Complexity;
                    }

                    var orderConfirmation = new OrderDetails
                    {
                        Id = order.Order.Id,
                        WaiterId = order.WaiterId,
                        TableId = order.TableId,
                        Priority = order.Order.Priority,
                        MaxWaitTime = order.Order.MaxWaitTime,
                        PickUpTime = DateTime.UtcNow
                    };

                    foreach (var food in StaticContext.FoodsToPrepare.Where(f => f.OrderId == order.Order.Id))
                    {
                        orderConfirmation.Items.Add(food.Food.Id);
                        orderConfirmation.CookingDetails.Add((food.Food.Id, StaticContext.Cooks.FirstOrDefault(c => c.Rank >= food.Food.Complexity).Id));
                    }

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
                        Console.WriteLine($"--> Finish preparing order: {order.Order.Id}");
                        var client = new HttpDataClient(_httpClient);
                        client.SendOrder(orderConfirmation);
                        //if (StaticContext.FoodsToPrepare.All(f => f.OrderId != order.Id))
                        //{
                        //    var client = new HttpDataClient(_httpClient);
                        //    client.SendOrder(orderConfirmation);
                        //    //return new SendOrderDto
                        //    //{
                        //    //    CreatedAt = order.CreatedAt,
                        //    //    Foods = order.Foods,
                        //    //    Id = order.Id,
                        //    //    MaxWaitTime = order.MaxWaitTime,
                        //    //    PreparedIn = DateTime.UtcNow.Subtract(order.ReceivedAt),
                        //    //    Priority = order.Priority,
                        //    //    ReceivedAt = order.ReceivedAt
                        //    //};
                        //}
                    }

                };
            }
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
