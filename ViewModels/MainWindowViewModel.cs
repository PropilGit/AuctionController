using AuctionController.Infrastructure.Commands.Base;
using AuctionController.Infrastructure.Selenium;
using AuctionController.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace AuctionController.ViewModels
{
    class MainWindowViewModel : ViewModel
    {

        public MainWindowViewModel()
        {
            _SeleniumController = SeleniumController.GetInstance();

            //_Command = new LambdaCommand(On_CommandExecuted, Can_CommandExecute);
            CheckSignatureEFRSBCommand = new LambdaCommand(OnCheckSignatureEFRSBCommandExecuted, CanCheckSignatureEFRSBCommandExecute);
        }

        #region Test

        #region Status

        string _Status = "...";
        public string Status { get => _Status; set => Set(ref _Status, value); }

        #endregion

        #region CheckSignatureEFRSBCommand 

        public ICommand CheckSignatureEFRSBCommand { get; }
        private bool CanCheckSignatureEFRSBCommandExecute(object p) => true;
        private void OnCheckSignatureEFRSBCommandExecuted(object p)
        {
            Status = _SeleniumController.CheckSignature_EFRSB();
        }

        #endregion

        #endregion

        #region Selenium

        SeleniumController _SeleniumController;

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