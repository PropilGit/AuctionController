using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionController.Models
{
    class Bet
    {
        public Bet(float summ, string name, DateTime time, bool isMine)
        {
            Summ = summ;
            Name = name;
            Time = time;
            IsMine = isMine;
        }

        public float Summ { get; private set; }
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public bool IsMine { get; private set; }

        public override string ToString()
        {
            return Summ + ", [" + Name + "] " + Time.ToString("HH:mm:ss") + IsMine;
        }
    }
}
