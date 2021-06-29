using AuctionController.Infrastructure.Selenium;
using AuctionController.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionController.ViewModels
{
    class MainWindowViewModel : ViewModel
    {

        public MainWindowViewModel()
        {
            _SeleniumController = SeleniumController.GetInstance();
        }

        #region Selenium

        SeleniumController _SeleniumController;

        #endregion

    }
}
