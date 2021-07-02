using AuctionController.Infrastructure.Commands.Base;
using AuctionController.Infrastructure.Selenium;
using AuctionController.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AuctionController.ViewModels
{
    class MainWindowViewModel : ViewModel
    {

        public MainWindowViewModel()
        {
            //_Command = new LambdaCommand(On_CommandExecuted, Can_CommandExecute);

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanStartCommandExecute);
            CheckSignatureMETSCommand = new LambdaCommand(OnCheckSignatureMETSCommandExecuted, CanCheckSignatureMETSCommandExecute);
        }

        #region Test

        #region Status

        string _Status = "...";
        public string Status { get => _Status; set => Set(ref _Status, value); }

        #endregion

        #region _
        #endregion

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
            _BlockInterface = false;
        }

        #endregion

        #region CheckSignatureMETSCommand 

        public ICommand CheckSignatureMETSCommand { get; }
        private bool CanCheckSignatureMETSCommandExecute(object p)
        {
            if (_BlockInterface) return false;
            if (_SeleniumController == null) return false;
            else return true;
        }
        async private void OnCheckSignatureMETSCommandExecuted(object p)
        {
            _BlockInterface = true;
            await Task.Run(() => CheckSignatureMETSAsync());
        }
        void CheckSignatureMETSAsync()
        {
            Status = _SeleniumController.CheckSignatureMETS();
            _BlockInterface = false;
        }

        #endregion

        #endregion

        #region Selenium

        SeleniumController _SeleniumController;

        bool _BlockInterface = false;

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