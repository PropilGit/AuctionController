using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionController.Infrastructure.Selenium
{
    class SeleniumController
    {
        #region Singleton

        private static SeleniumController instance;
        public static SeleniumController GetInstance()
        {
            if (instance == null) instance = new SeleniumController();
            return instance;
        }

        #endregion

        public SeleniumController()
        {
            IWebDriver driver = new InternetExplorerDriver();
            driver.Navigate().GoToUrl("http://yandex.ru");
        }
    }
}
