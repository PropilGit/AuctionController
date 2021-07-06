using AuctionController.Infrastructure.Commands.Base;
using AuctionController.Infrastructure.JSON;
using AuctionController.Infrastructure.Selenium;
using AuctionController.Models;
using AuctionController.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            //_Command = new LambdaCommand(On_CommandExecuted, Can_CommandExecute);

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanStartCommandExecute);
            CheckMETSCommand = new LambdaCommand(OnCheckMETSCommandExecuted, CanCheckMETSCommandExecute);
            ChangeWaitTimeCommand = new LambdaCommand(OnChangeWaitTimeCommandExecuted, CanChangeWaitTimeCommandExecute);
            //GetLotsAURU
            GetLotsAURUCommand = new LambdaCommand(OnGetLotsAURUCommandExecuted, CanGetLotsAURUCommandExecute);
        }

        #region AUs

        string AUPath = "AUs.json";

        public ObservableCollection<ArbitralManager> AUs { get; private set; }
        public ArbitralManager SelectedAU { get; set; } 

        void LoadAUList()
        {
            AUs = JSONConverter.OpenJSONFile<ObservableCollection<ArbitralManager>>(AUPath);
            if (AUs == null || AUs.Count == 0)
            {
                AUs = new ObservableCollection<ArbitralManager>();
                SelectedAU = null;
                AddLog("Ошибка загрузки списка Арбитражных Управляющих", true);
            }
            else SelectedAU = AUs[0];
        }

        #endregion

        #region Lots

        List<int> LotIds = new List<int>()
        {
            17841129,
            17841270,
            17011962,
            13748863,
            13742798,
            7725899,
            17731701,
            17843855,
            17843854,
            15661860
        };

        public ObservableCollection<Lot> Lots;

        #region GetLotsAURUCommand 

        public ICommand GetLotsAURUCommand { get; }
        private bool CanGetLotsAURUCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            else return true;
        }
        async private void OnGetLotsAURUCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => GetLotsAURUAsync());
        }
        void GetLotsAURUAsync()
        {
            Lots = _SeleniumController.ParseLots_AURU(LotIds);
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

        int _WaitTime = 10;
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
            if (SelectedAU == null) return false;
            else return true;
        }
        async private void OnCheckMETSCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => CheckMETSAsync());
        }     
        void CheckMETSAsync()
        {
            bool login = true;// _SeleniumController.Login_METS_MF(SelectedAU);
            bool sighature = _SeleniumController.CheckSignature_METS_MF(SelectedAU.Name);

            if (login == true && sighature == true)
            {
                Checked = true;
                Status = "[" + SelectedAU.Name + "] Вход в Личный кабинет и проверка ЭЦП завершились успешно";
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

        public void AddLog(string msg, bool isError = false)
        {
            //если есть пометка об ошибке
            if (isError)
            {
                msg = "ERROR: " + msg;
                Status = msg;
                //_TelegramBot.SendMessageToChat(msg, GetChatId(debugChat.Tag));
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

    }
}



#region _
#endregion
/*
#region _Command 

public ICommand _Command { get; }
private bool Can_CommandExecute(object p) => true;
private void On_CommandExecuted(object p)
{
    
}

#endregion
*/