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
        public Lot(string id, int number, string name, float startPrice)
        {
            Id = id;
            Number = number;
            Name = name;
            StartPrice = startPrice;
            CurrentPrice = startPrice;
        }
        public static Lot Error(string id)
        {
            return new Lot(id, 0, "Error", 0);
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
        public float PriceDifference { get => CurrentPrice - StartPrice; }
        public float PriceDifferenceProc { get => (float)Math.Round((CurrentPrice - StartPrice) / (StartPrice / 100), 1); }
        //public DateTime StartDate { get; private set; }
        public DateTime EndTime { get; private set; }
        public TimeSpan RemainingTime { get; private set; }
        public float CurrentPrice { get; private set; }
        //public string PriceDifference { get; private set; }

        public bool Update(DateTime endTime, float currentPrice, Bet[] newBets)
        {
            try
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    EndTime = endTime;
                    OnPropertyChanged("EndTime");

                    CurrentPrice = currentPrice;
                    OnPropertyChanged("CurrentPrice");
                    OnPropertyChanged("PriceDifference");
                    OnPropertyChanged("PriceDifferenceProc");

                    OnPropertyChanged("RemainingTime");

                    for (int i = 0; i < Bets.Count; i++)
                    {
                        Bets[i] = newBets[i];
                    }
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateCurrentBet(Bet newBet)
        {
            /**
            Bets.RemoveAt(3);
            Bets.Insert(0, newBet);

            if (Bets[0].Summ - StartPrice > 0)
            {
            PriceDifference = Bets[0].Summ - StartPrice;
            OnPropertyChanged("PriceDifference");
            PriceDifferenceProc = (float)Math.Round((Bets[0].Summ - StartPrice) / (StartPrice / 100), 1);
            OnPropertyChanged("PriceDifferenceProc");
            OnPropertyChanged("RemainingTime");
            }
            */
            return true;
        }

        public void UpdateRemainingTime()
        {
            OnPropertyChanged("RemainingTime");
        }
    }
}
