using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitchen.Models;

namespace Kitchen.Data
{
    public static class StaticContext
    {
        public static ThreadSafeListWithLock<Order> Orders { get; set; } = new ThreadSafeListWithLock<Order>();

        public static ThreadSafeListWithLock<Food> Foods { get; set; } = new ThreadSafeListWithLock<Food>();

        public static ThreadSafeListWithLock<Cook> Cooks { get; set; } = new ThreadSafeListWithLock<Cook>();

        public static ThreadSafeListWithLock<CookingApparatus> CookingApparatuses { get; set; } =
            new ThreadSafeListWithLock<CookingApparatus>();

        public static ThreadSafeListWithLock<OrderFood> FoodsToPrepare { get; set; } = new ThreadSafeListWithLock<OrderFood>();

        public static int orderNr = -1;
    }
}
