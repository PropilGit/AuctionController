using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionController.Models
{
    class Lot
    {
        public Lot(int id, int number, string name, string link, double currentRate, DateTime startDate)
        {
            Id = id;
            Number = number;
            Name = name;
            Link = link;
            CurrentRate = currentRate;
            StartDate = startDate;
        }

        public int Id { get; private set; }
        public int Number { get; private set; }
        public string Name { get; private set; }
        public string Link { get; private set; }
        public double CurrentRate { get; private set; }
        //public double Step { get; private set; }
        public DateTime StartDate { get; private set; }

        public bool UpdateCurrentRate(double newRate)
        {
            if (CurrentRate > newRate) return false;

            CurrentRate = newRate;
            return true;
        }
    }
}
