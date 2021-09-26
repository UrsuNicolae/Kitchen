using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public sealed class OrderList
    {
        public IEnumerable<Order> Orders { get; set; }
    }
}
