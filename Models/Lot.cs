using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuctionController.Models
{
    class Lot : INotifyPropertyChanged
    {
        public Lot(string id, int number, string name, float startPrice, DateTime startDate)
        {
            Id = id;
            Number = number;
            Name = name;
            StartPrice = startPrice;
            StartDate = startDate;
        }
        public static Lot Error(string id)
        {
            return new Lot(id, 0, "Error", 0, DateTime.Now);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool Checked { get; set; }
        public int Index { get; set; }
        public string Id { get; private set; }
        public int Number { get; private set; }
        public string Name { get; private set; }
        public float StartPrice { get; private set; }
        public ObservableCollection<Bet> Bets { get; private set; } = new ObservableCollection<Bet>
        {
            new Bet(0, "---", DateTime.UnixEpoch),
            new Bet(0, "---", DateTime.UnixEpoch),
            new Bet(0, "---", DateTime.UnixEpoch),
            new Bet(0, "---", DateTime.UnixEpoch)
        };
        public float PriceDifference { get; private set; } = 0;
        public float PriceDifferenceProc { get; private set; } = 0;
        public DateTime StartDate { get; private set; }

        public bool UpdateCurrentBet(Bet newBet)
        {
            Bets.RemoveAt(3);
            Bets.Insert(0, newBet);

            if (Bets[0].Summ - StartPrice > 0)
            {
                PriceDifference = Bets[0].Summ - StartPrice;
                OnPropertyChanged("PriceDifference");
                PriceDifferenceProc = (float)Math.Round((Bets[0].Summ - StartPrice) / (StartPrice / 100), 1);
                OnPropertyChanged("PriceDifferenceProc");
            }

            return true;
        }
    }
}
