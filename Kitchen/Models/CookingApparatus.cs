using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public class CookingApparatus
    {
        public Guid Id { get; set; }

        public CookingApparatuses TypeOfApparatus { get; set; }

        public bool IsFree { get; set; }
    }
}
