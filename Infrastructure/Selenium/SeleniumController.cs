using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
            driver = new InternetExplorerDriver();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        ~SeleniumController()
        {
            driver.Close();
        }

        public string CheckSignatureMETS(string auName = "Хамидулин Илья Хамитович")
        {
            
            try
            {
                // 1 Переходим на страницу
                driver.Navigate().GoToUrl("https://m-ets.ru/signTest");

                // 2 Кликаем по кнопке
                wait.Until(e => e.FindElement(By.XPath("//button[@id='submitbtn']"))).SendKeys(Keys.Enter);

                // Закрываем уведомление
                //wait.Until(e => e.FindElement(By.ClassName("pluginDialog_close"))).Click();

                // 3 Получаем элементы из выпадающего списка
                var certificates = wait.Until(e => e.FindElements(By.XPath("//select[@id='certSel']/option")));

                // 4 Кликаем по выпадающему списку
                wait.Until(e => e.FindElement(By.XPath("//select[@id='certSel']"))).SendKeys(Keys.Enter);

                // 5 
                foreach (var cert in certificates)
                {
                    if (cert.Text.Contains(auName))
                    {
                        cert.SendKeys(Keys.Enter);
                        break;
                    }   
                }

                // 6
                //string xpath = @"//div[contains(@role, 'dialog')]/div[contains(@class, 'ui-dialog-buttonpane')]/div[contains(@class, 'ui-dialog-buttonset')]/button[1]";
                string xpath = "/html/body/div[20]/div[11]/div/button[1]";
                wait.Until(e => e.FindElement(By.XPath(xpath))).SendKeys(Keys.Enter);

                // 7 
                /*
                do
                {
                    
                }while()

                

                xpath = "/html/body/div[13]/div[3]/div[2]/div/div[5]/p/span";
                var el = wait.Until(e => e.FindElement(By.XPath(xpath)));
                if (el.Text.Contains(auName)) return "OK";
                else return "FAIL";
                */
            }
            catch (Exception ex)
            {
                return "ERROR: Исключение:" + ex.Message;
            }
        }
    }
}
