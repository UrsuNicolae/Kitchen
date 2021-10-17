using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Domain.Model;

namespace Kitchen.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ServeController : ControllerBase
    {
        private readonly ILogger<ServeController> _logger;
        private readonly Cook[] _cooks;

        public ServeController(ILogger<ServeController> logger)
        {
            _cooks = new Cook[]
            {
                new Cook(), new Cook()
            };
            _logger = logger;
        }

        [HttpGet]
        public int Tables()
        {
            return new Random().Next(5, 10);
        }

        [HttpPost]
        public string Order( Order order)
        {
            if (!ModelState.IsValid)
            {
                return "Order not received";
            }

            while (true)
            {
                var cook = _cooks.FirstOrDefault(c => c.IsAvailable);
                if (cook != null)
                {
                    _logger.LogInformation("Order is received");
                    //cook.PrepareOrder(order);
                    return "Order recieved";
                }
            }
        }
    }
}
