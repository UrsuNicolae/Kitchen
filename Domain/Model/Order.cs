using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public sealed class Order
    {
        public Order(IEnumerable<Food> foods)
        {
            Foods = foods;
        }
        //public bool Taken { get; set; } = false;
        public IEnumerable<Food> Foods { get; set; }
    }
}
