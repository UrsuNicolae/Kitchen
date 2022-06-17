using Kitchen.Data;
using Kitchen.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public sealed class Cook
    {
        [Key]
        public int Id { get; set; }

        public bool IsAvailable { get; set; }

        public int Rank { get; set; }

        public int Proficiency { get; set; }


        public void StartPreparing(OrderFood orderFood)
        {
            new Thread(() =>
            {
                var food = orderFood.Food;
                Thread.CurrentThread.IsBackground = true;
                IsAvailable = false;
                var proficiency = Proficiency;


                if (food.CookingApparatus == CookingApparatuses.None)
                {
                    Console.WriteLine($"--> Start preparing food: {food.Name} ");
                    if (proficiency == 0)
                    {
                        Thread.Sleep(food.PreparationTime * 100); //preparing food
                        proficiency = Proficiency;
                    }
                    else
                    {
                        proficiency--;
                    }

                    Console.WriteLine($"-->Finish preparing food: {food.Name} ");
                }
                else
                {
                    bool available = false;
                    CookingApparatus cookingApparatus = null;
                    while (!available)
                    {
                        cookingApparatus = StaticContext.CookingApparatuses.FirstOrDefault(c =>
                                        c.TypeOfApparatus == food.CookingApparatus && c.IsFree);
                        available = cookingApparatus != null;
                    }
                    cookingApparatus.IsFree = false;

                    Console.WriteLine($"--> Start preparing food: {food.Name}, on {cookingApparatus.TypeOfApparatus.ToString()}");
                    if (proficiency == 0)
                    {
                        Thread.Sleep(food.PreparationTime * 100); //preparing food
                        proficiency = Proficiency;
                    }
                    else
                    {
                        proficiency--;
                    }

                    Thread.Sleep(food.PreparationTime * 100); //preparing food
                    Console.WriteLine($"-->Finish preparing food: {food.Name}, on {cookingApparatus.TypeOfApparatus.ToString()}");
                    cookingApparatus.IsFree = true;
                }
                IsAvailable = true;
            }).Start();
        }
    }
}
