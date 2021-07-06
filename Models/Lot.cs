using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionController.Models
{
    class Lot
    {
        public Lot(int id, int number, string name, double currentPrice, DateTime startDate)
        {
            Id = id;
            Number = number;
            Name = name;
            //Link = link;
            CurrentPrice = currentPrice;
            StartDate = startDate;
        }

        public int Id { get; private set; }
        public int Number { get; private set; }
        public string Name { get; private set; }
        //public string Link { get; private set; }
        public double CurrentPrice { get; private set; }
        //public double Step { get; private set; }
        public DateTime StartDate { get; private set; }

        public bool UpdateCurrentPrice(double newPrice)
        {
            if (CurrentPrice > newPrice) return false;

            CurrentPrice = newPrice;
            return true;
        }
    }
}
