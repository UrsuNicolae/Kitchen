using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public sealed class OrderList
    {
        public IList<Order> Orders { get; set; }
    }
}
