using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitchen.Models;

namespace Kitchen.DTOs
{
    public class SendOrderDto: Order
    {
        public TimeSpan PreparedIn { get; set; }
    }
}
