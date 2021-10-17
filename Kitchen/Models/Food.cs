using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public sealed class Food
    {
        public Food(string name, TimeSpan preparationTime, int complexity, CookingApparatuses cookingApparatus)
        {
            Id = new Guid();
            Name = name;
            PreparationTime = preparationTime;
            Complexity = complexity;
            CookingApparatus = cookingApparatus;
        }
        public Guid Id { get; }

        public string Name { get; set; }

        public TimeSpan PreparationTime { get; set; }

        public int Complexity { get; set; }

        public CookingApparatuses CookingApparatus { get; set; }
    }
}
