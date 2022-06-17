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


        public SendOrderDto StartPreparing(Order curentOrder)
        {
            SendOrderDto orderToReturn = null;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                IsAvailable = false;
                var proficiency = Proficiency;
                Console.WriteLine($"--> Start preparing order {curentOrder.Id}");
                Parallel.ForEach(curentOrder.Foods, food =>
                {
                    if (food.CookingApparatus == CookingApparatuses.None)
                    {
                        Console.WriteLine($"--> Start preparing food: {food.Name}");
                        if (proficiency == 0)
                        {
                            Thread.Sleep(food.PreparationTime * 100); //preparing food
                            proficiency = Proficiency;
                        }
                        else
                        {
                            proficiency--;
                        }

                        Console.WriteLine($"-->Finish preparing food: {food.Name}");
                    }
                    else
                    {
                        while (StaticContext.CookingApparatuses.FirstOrDefault(c =>
                                        c.TypeOfApparatus == food.CookingApparatus) == null)
                        {
                            Console.WriteLine(
                                            $"--> Waiting for {Enum.GetName(food.CookingApparatus.GetType(), food.CookingApparatus)}");
                        }

                        var cookingApparatus = StaticContext.CookingApparatuses.FirstOrDefault(c =>
                                            c.TypeOfApparatus == food.CookingApparatus);
                        cookingApparatus.IsFree = false;
                        Console.WriteLine($"--> Start preparing food: {food.Name}");
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
                        Console.WriteLine($"-->Finish preparing food: {food.Name}");
                        cookingApparatus.IsFree = true;
                    }
                });
                Console.WriteLine($"--> Finish preparing order: {curentOrder.Id}");
                IsAvailable = true;
                orderToReturn = new SendOrderDto
                {
                    CreatedAt = curentOrder.CreatedAt,
                    Foods = curentOrder.Foods,
                    Id = curentOrder.Id,
                    MaxWaitTime = curentOrder.MaxWaitTime,
                    PreparedIn = DateTime.UtcNow.Subtract(curentOrder.ReceivedAt),
                    Priority = curentOrder.Priority,
                    ReceivedAt = curentOrder.ReceivedAt
                };
            }).Start();
            return orderToReturn;
        }
    }
}
