using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

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

        /*
        public delegate void StartTask();
        public event StartTask onStartTask;

        public delegate void EndTask();
        public event StartTask onEndTask;
        */

        IWebDriver driver;
        WebDriverWait wait;

        public SeleniumController()
        {
            //driver = new InternetExplorerDriver();

            //FirefoxProfile profile = new FirefoxProfile();
            //profile.AddExtension("firefox_cryptopro_extension.xpi");

            //FirefoxOptions options = new FirefoxOptions();
            //options.Profile.AddExtension(@"firefox_cryptopro_extension.xpi");

            driver = new InternetExplorerDriver();
            
            //driver.install_addon('firefox_cryptopro_extension.xpi', temporary = True);
            

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        ~SeleniumController()
        {
            driver.Close();
        }

        public string CheckSignatureMETS(string auName = "Ляхов Андрей Николаевич")
        {
            
            try
            {
                // 1 Переходим на страницу
                driver.Navigate().GoToUrl("https://m-ets.ru/signTest");

                // 2 Кликаем по кнопке
                wait.Until(e => e.FindElement(By.XPath("//button[@id='submitbtn']"))).Click();

                // 3 Кликаем по кнопке
                wait.Until(e => e.FindElement(By.ClassName("pluginDialog_close"))).Click();

                // 4 Получаем элементы из выпадающего списка
                var certificates = wait.Until(e => e.FindElements(By.XPath("//select[@id='certSel']/option")));

                // 5 Кликаем выпадающего списка
                wait.Until(e => e.FindElement(By.XPath("//select[@id='certSel']"))).Click();

                // 6 
                foreach (var cert in certificates)
                {
                    if (cert.Text == auName) cert.Click();
                    break;
                }
                /*
                IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());
                if (alert.Text == "Электронная подпись успешно прошла проверку.")
                {
                    alert.Accept();
                    return "OK";
                }
                else return "FAIL: " + alert.Text;
                return "ERROR: На этапе получения Alert";
                */
                return "test";
            }
            catch (Exception ex)
            {
                return "ERROR: Исключение:" + ex.Message;
            }
        }
    }
}
