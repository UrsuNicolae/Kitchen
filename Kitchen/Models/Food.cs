using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public sealed class Food
    {
        [Key]
        public Guid Key { get; set; } = new Guid();
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int PreparationTime { get; set; }

        [Required]
        public int Complexity { get; set; }

        [Required]
        public CookingApparatuses CookingApparatus { get; set; }
    }
}
