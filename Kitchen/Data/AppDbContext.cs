using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitchen.Models;
using Microsoft.EntityFrameworkCore;

namespace Kitchen.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Food> Foods { get; set; }

        public DbSet<Cook> Cooks { get; set; }

        public DbSet<CookingApparatus> CookingApparatuses { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(o => o.Id)
                .ValueGeneratedNever();
        }
    }
}
