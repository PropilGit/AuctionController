using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AuctionController.Infrastructure
{
    class Clock
    {
        #region Singleton

        static Clock instance;
        public static Clock GetInstance()
        {
            if (instance == null) instance = new Clock();
            return instance;
        }

        #endregion

        DispatcherTimer dispatcherTimer;
        public int Hour { get; private set; }
        public int Min { get; private set; }

        public delegate void UpdateTime();
        public event UpdateTime onTimeUpdate;

        public Clock()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            SetInterval(new TimeSpan(0, 0, 5));

            Update();

            SynchronizeClockAsync();
        }

        public void SetInterval(TimeSpan interval)
        {
            dispatcherTimer.Interval = interval;
        }

        async void SynchronizeClockAsync()
        {
            await Task.Run(() => SynchronizeClock());
        }

        void SynchronizeClock()
        {
            int delta = 60 * 1000 - DateTime.Now.Second * 1000;
            Thread.Sleep(delta);

            dispatcherTimer.Start();
            Update();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        public void Update()
        {
            if (onTimeUpdate != null) onTimeUpdate();
        }
    }
}
