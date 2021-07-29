using AuctionController.Infrastructure;
using AuctionController.Infrastructure.Commands.Base;
using AuctionController.Infrastructure.JSON;
using AuctionController.Infrastructure.Selenium;
using AuctionController.Infrastructure.Telegram;
using AuctionController.Models;
using AuctionController.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AuctionController.ViewModels
{
    class MainWindowViewModel : ViewModel
    {

        public MainWindowViewModel()
        {
            LoadAUList();

            _Clock = InitializeClock();
            //_TelegramBot = InitializeTelegramBot();

            //_Command = new LambdaCommand(On_CommandExecuted, Can_CommandExecute);

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanStartCommandExecute);
            CheckMETSCommand = new LambdaCommand(OnCheckMETSCommandExecuted, CanCheckMETSCommandExecute);
            ChangeWaitTimeCommand = new LambdaCommand(OnChangeWaitTimeCommandExecuted, CanChangeWaitTimeCommandExecute);

            GetSelectedLotsCommand = new LambdaCommand(OnGetSelectedLotsCommandExecuted, CanGetSelectedLotsCommandExecute);
            GetAllLotsCommand = new LambdaCommand(OnGetAllLotsCommandExecuted, CanGetAllLotsCommandExecute);
            ClearLotsCommand = new LambdaCommand(OnClearLotsCommandExecuted, CanClearLotsCommandExecute);
            UpdateLotsCommand = new LambdaCommand(OnUpdateLotsCommandExecuted, CanUpdateLotsCommandExecute);
            ReloadLotsCommand = new LambdaCommand(OnReloadLotsCommandExecuted, CanReloadLotsCommandExecute);
            MakeBetsCommand = new LambdaCommand(OnMakeBetsCommandExecuted, CanMakeBetsCommandExecute);
        }

        #region Bidders

        string BiddersPath = "bidders.json";

        public ObservableCollection<Bidder> Bidders { get; private set; }
        public Bidder SelectedBidder { get; set; } 

        void LoadAUList()
        {
            Bidders = JSONConverter.OpenJSONFile<ObservableCollection<Bidder>>(BiddersPath);
            if (Bidders == null || Bidders.Count == 0)
            {
                Bidders = new ObservableCollection<Bidder>();
                SelectedBidder = null;
                AddLog("Ошибка загрузки списка Арбитражных Управляющих", true);
            }
            else SelectedBidder = Bidders[0];
        }

        #endregion

        #region Lots

        #region AuctionID

        string _AuctionID = "252073694";
        public string AuctionID { get => _AuctionID; set => Set(ref _AuctionID, value); }

        #endregion

        #region AutoBet

        public bool AutoBet { get; set; } = false;

        TimeSpan _AutoBetTime = new TimeSpan(0, 5, 0);

        #endregion

        #region SelectedLots

        List<int> _SelectedLotsNumbers = new List<int>() { 2 };

        public string SelectedLotsNumbers
        {
            get
            {
                if (_SelectedLotsNumbers == null || _SelectedLotsNumbers.Count == 0) return "";

                string result = "";
                foreach (var num in _SelectedLotsNumbers)
                {
                    result += num.ToString() + ", ";
                }
                result = result.Substring(0, result.Length - 1);
                return result;
            }
            set
            {
                try
                {
                    if (value == "") return;
                    string[] numbers = value.Split(",");

                    if (numbers == null || numbers.Length == 0) return;
                    _SelectedLotsNumbers.Clear();
                    foreach (var num in numbers)
                    {
                        _SelectedLotsNumbers.Add(Int32.Parse(num));
                    }
                }
                catch (Exception ex)
                {
                    //AddLog("Не удалось получить список лотов");
                    //SelectedLotsNumbers = "";
                }
            }
        }

        #endregion

        #region BetTimer, AutoUpdateLots

        public bool AutoUpdateLots { get; set; } = false;
        public ObservableCollection<int> BetTimer { get; private set; } = new ObservableCollection<int>() { 29, 20, 10, 5, 4, 3, 2, 1 };

        public int _SelectedBetTimer = 5;
        public int SelectedBetTimer
        {
            get => _SelectedBetTimer;
            set
            {
                _AutoBetTime = new TimeSpan(0, value, 0);
                _SelectedBetTimer = value;
            }
        }

        #endregion

        #region Lots

        ObservableCollection<Lot> _Lots = new ObservableCollection<Lot>();
        public ObservableCollection<Lot> Lots
        {
            get => _Lots;
            set
            {
                if (value == null) return;

                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i] == null) value[i] = Lot.Error("");
                    value[i].Index = i;
                }

                Set(ref _Lots, value);
            }
        }

        #endregion

        #region GetSelectedLotsCommand

        public ICommand GetSelectedLotsCommand { get; }

        private bool CanGetSelectedLotsCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            if (Lots != null && Lots.Count > 0) return false;
            if (_SelectedLotsNumbers == null || _SelectedLotsNumbers.Count == 0) return false;
            
            return true;
        }
        async private void OnGetSelectedLotsCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => GetSelectedLotsAsync());
        }
        void GetSelectedLotsAsync()
        {
            Lots = _SeleniumController.ParseSelectedLotsOnPage_METS_MF(AuctionID, _SelectedLotsNumbers);
            _BlockInterface = false;
        }

        #endregion

        #region GetAllLotsCommand

        public ICommand GetAllLotsCommand { get; }

        private bool CanGetAllLotsCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            if (Lots != null && Lots.Count > 0) return false;
            else return true;
        }
        async private void OnGetAllLotsCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => GetAllLotsAsync());
        }
        void GetAllLotsAsync()
        {
            Lots = _SeleniumController.ParseAllLotsOnPage_METS_MF(AuctionID);
            _BlockInterface = false;
        }

        #endregion

        #region ClearLotsCommand

        public ICommand ClearLotsCommand { get; }

        private bool CanClearLotsCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (Lots == null || Lots.Count == 0) return false;
            else return true;
        }
        private void OnClearLotsCommandExecuted(object p)
        {
            List<Lot> choosenLots = new List<Lot>();
            foreach (var lot in Lots)
            {
                if (lot.Checked) choosenLots.Add(lot);
            }

            if (choosenLots == null || choosenLots.Count == 0)
            {
                Lots = new ObservableCollection<Lot>();
            }
            else
            {
                foreach (var clot in choosenLots) 
                {
                    Lots.Remove(clot);
                }
            }
        }

        #endregion

        #region UpdateLotsCommand

        public ICommand UpdateLotsCommand { get; }

        private bool CanUpdateLotsCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            if (Lots == null || Lots.Count == 0) return false;
            else return true;
        }
        async private void OnUpdateLotsCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => UpdateLotsAsync());
        }
        void UpdateLotsAsync()
        {
            do
            {
                foreach (var lot in Lots)
                {
                    lot.Indication = ">>>";
                    if (_SeleniumController.UpdateLot_METS_MF(lot))
                    {
                        if (AutoBet && lot.RemainingTime <= _AutoBetTime && !lot.Bets[0].IsMine)
                        {
                            _SeleniumController.MakeBet_METS_MF(lot, SelectedBidder);
                            _SeleniumController.UpdateLot_METS_MF(lot);
                        }
                    }
                    lot.Indication = "";
                }
            } while (AutoUpdateLots);
            
            _BlockInterface = false;
        }

        #endregion

        #region ReloadLotsCommand
        
        public ICommand ReloadLotsCommand{ get; }

        private bool CanReloadLotsCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            if (Lots == null || Lots.Count == 0) return false;
            else return true;
        }
        async private void OnReloadLotsCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => ReloadLotsAsync());
        }
        void ReloadLotsAsync()
        {
            for (int i = 0; i < Lots.Count; i++)
            {
                if (!Lots[i].Checked) continue;

                Lot newLot = _SeleniumController.ParseSingleLot_METS_MF(AuctionID, Lots[i].Id);
                if (newLot == null || newLot.Name == "Error")
                {
                    AddLog("Не удалось перезагрузить лот " + Lots[i].Id);
                    continue;
                }

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Lots[i] = newLot;
                });
            }

            _BlockInterface = false;
        }
        
        #endregion

        #region MakeBetsCommand

        public ICommand MakeBetsCommand { get; }

        private bool CanMakeBetsCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            if (Lots == null || Lots.Count == 0) return false;
            else return true;
        }
        async private void OnMakeBetsCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => MakeBetsAsync());
        }
        void MakeBetsAsync()
        {
            foreach (var lot in Lots)
            {
                if (!lot.Checked) continue;

                if (_SeleniumController.MakeBet_METS_MF(lot, SelectedBidder))
                {
                    _SeleniumController.UpdateLot_METS_MF(lot);
                }
                else
                {
                    AddLog("MakeBetsAsync(): Не удалось сделать ставку (лот: " + lot.Id + ")", true);
                }
                lot.Checked = false;
            }

            _BlockInterface = false;
        }

        #endregion

        #endregion

        #region Selenium

        SeleniumController _SeleniumController;

        bool _BlockInterface = false;

        public bool UseSendKeys
        {
            //get => _SeleniumController.UseSendKeys; set => Set(ref _SeleniumController.UseSendKeys, value);
            get
            {
                if (_SeleniumController == null) return false;
                else return _SeleniumController.UseSendKeys;
            }
            set
            {
                if (_SeleniumController == null) return;
                else Set(ref _SeleniumController.UseSendKeys, value);
            }
        }

        #region WaitTime

        int _WaitTime = 5;
        public int WaitTime
        {
            get => _WaitTime;
            set
            {
                if (value < 1 || value > 60) return;
                else Set(ref _WaitTime, value);
            }
        }

        #region ChangeWaitTimeCommand 

        public ICommand ChangeWaitTimeCommand { get; }
        private bool CanChangeWaitTimeCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            return true;
        }
        async private void OnChangeWaitTimeCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => ChangeWaitTimeAsync());
        }
        void ChangeWaitTimeAsync()
        {
            _SeleniumController.UpdateWebDriverWait(WaitTime);
            _BlockInterface = false;
        }

        #endregion

        #endregion

        #region Start

        bool _Started = false;
        public bool Started { get => _Started; set => Set(ref _Started, value); }

        #region StartCommand 

        public ICommand StartCommand { get; }
        private bool CanStartCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController != null) return false;
            else return true;
        }
        async private void OnStartCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => InstantiateSeleniumAsync());
        }
        void InstantiateSeleniumAsync()
        {
            _SeleniumController = SeleniumController.GetInstance();
            _SeleniumController.onLogUpdate += AddLog;
            Started = true;
            _BlockInterface = false;
        }

        #endregion

        #endregion

        #region Check

        bool _Checked = false;
        public bool Checked { get => _Checked; set => Set(ref _Checked, value); }

        #region CheckMETSCommand

        public ICommand CheckMETSCommand { get; }
        private bool CanCheckMETSCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            if (SelectedBidder == null) return false;
            else return true;
        }
        async private void OnCheckMETSCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => CheckMETSAsync());
        }     
        void CheckMETSAsync()
        {
            bool login = _SeleniumController.Login_METS_MF(SelectedBidder);
            bool sighature = _SeleniumController.CheckSignature_METS_MF(SelectedBidder.Name);


            if (login && sighature)
            {
                Checked = true;
                Status = "[" + SelectedBidder.Name + "] Вход в Личный кабинет и проверка ЭЦП завершились успешно";
            }
            _BlockInterface = false;
        }

        #endregion

        #endregion

        #endregion

        #region Status

        string _Status = "...";
        public string Status { get => _Status; set => Set(ref _Status, value); }

        #endregion

        #region Log

        int maxLogValue = 150;
        int logCounter = 0;

        private string _Log = "";
        public string Log { get => _Log; set => Set(ref _Log, value); }

        public void AddLog(string msg, bool isError = true)
        {
            //если есть пометка об ошибке
            if (isError)
            {
                msg = "ERROR: " + msg;
                Status = msg;
                //_TelegramBot.SendMessageToChat(msg, DebugChatId);
            }
            Log += "[" + DateTime.Now + "] " + msg + "\r\n";
            logCounter++;
            
            //Очищаем лог
            if (logCounter >= maxLogValue)
            {
                logCounter = 0;
                Log = "";
            }
        }

        #endregion

        #region Clock

        Clock _Clock;

        Clock InitializeClock()
        {
            try
            {
                Clock clock = Clock.GetInstance();

                clock.onTimeUpdate += OnTimeUpdate;
                //clock.onTimeUpdate += CheckAlertTasks;

                OnTimeUpdate();
                return clock;
            }
            catch (Exception ex)
            {
                AddLog("InitializeClock()" + ex.Message, true);
                return null;
            }

        }
        void OnTimeUpdate()
        {
            foreach (var lot in Lots)
            {
                lot.UpdateRemainingTime();
            }
        }

        #endregion

        #region TelegramBot

        TelegramBot _TelegramBot;

        TelegramBot InitializeTelegramBot()
        {
            TelegramBot newTelegramBot = TelegramBot.GetInstance();
            newTelegramBot.onLogUpdate += AddLog;
            //newTelegramBot.onCommandExecute += TelegramBotCommandExecute;

            return newTelegramBot;
        }

        #region ChatId

        public long ChatId { get; private set; } = 0;
        public long DebugChatId { get; private set; } = -1001320796606;

        string chatPath = "chatId.json";
        void LoadChatsList()
        {
            ChatId = JSONConverter.OpenJSONFile<long>(chatPath);
            if (ChatId == 0)
            {
                AddLog("Ошибка загрузки id чата.", true);
            }
        }

        #endregion

        #endregion

    }
}



#region _
#endregion