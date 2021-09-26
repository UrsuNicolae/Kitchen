using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public sealed class Order
    {
        public IEnumerable<Food> Foods { get; set; }
    }
}
