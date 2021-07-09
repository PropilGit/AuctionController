using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionController.Models
{
    class Lot
    {
        public Lot(string id, int number, string name, float currentPrice, DateTime startDate)
        {
            Id = id;
            Number = number;
            Name = name;
            //Link = link;
            CurrentPrice = currentPrice;
            StartDate = startDate;
        }

        public static Lot Error(string id)
        {
            return new Lot(id, 0, "Error", 0, DateTime.Now);
        }

        public string Id { get; private set; }
        public int Number { get; private set; }
        public string Name { get; private set; }
        //public string Link { get; private set; }
        public float CurrentPrice { get; private set; }
        //public double Step { get; private set; }
        public DateTime StartDate { get; private set; }

        public bool UpdateCurrentPrice(float newPrice)
        {
            if (CurrentPrice > newPrice) return false;

            CurrentPrice = newPrice;
            return true;
        }
    }
}
