using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;

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

        IWebDriver driver;
        WebDriverWait wait;

        public SeleniumController()
        {
            //FirefoxProfile profile = new FirefoxProfileManager().GetProfile("m-ets");
            //string prof = "C:\\Users\\MinhPhuc\\AppData\\Local\\Mozilla\\Firefox\\Profiles\\tiqq1wks.dev-edition-default\\";
            //string prof = "u74kume0.m-ets";
            //FirefoxProfile profile = new FirefoxProfile(prof);
            //FirefoxOptions options = new FirefoxOptions();
            //options.Profile = profile;
            //driver = new FirefoxDriver(options);

            // IE
            driver = new InternetExplorerDriver();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        ~SeleniumController()
        {
            driver.Close();
        }

        #region Log

        public delegate void UpdateLog(string msg, bool isError = false);
        public event UpdateLog onLogUpdate;
        void AddLog(string msg, bool isError = false)
        {
            onLogUpdate?.Invoke(msg, isError);
        }

        #endregion

        #region Try

        #region Click

        public bool UseSendKeys = false; // некоторые браузеры не принимают команду Click() - нужно использовать SendKeys()
        private bool TestClick()
        {
            try
            {
                // 1 Переходим на страницу
                driver.Navigate().GoToUrl("https://m-ets.ru/signTest");

                // 2 Кликаем по кнопке
                wait.Until(e => e.FindElement(By.XPath("//button[@id='submitbtn']"))).Click();

                // 3 Кликаем по кнопке отмена
                wait.Until(e => e.FindElement(By.XPath("/html/body/div[20]/div[11]/div/button[2]"))).Click();

                return false;
            }
            catch (Exception ex)
            {
                if(ex.Message == "Element cannot be interacted with via the keyboard because it is not displayed")
                {
                    return true;
                }
            }

            return true;
        }
        private bool TryClickOnElement(string xPath)
        {
            try
            {
                if (UseSendKeys) wait.Until(e => e.FindElement(By.XPath(xPath))).SendKeys(Keys.Enter);
                else wait.Until(e => e.FindElement(By.XPath(xPath))).Click();
                return true;
            }
            catch (Exception ex)
            {
                AddLog("TryClickOnElement(" + xPath + "): " + ex.Message, true);
                return false;
            }
        }
        private bool TryClickOnElement(IWebElement element)
        {
            try
            {
                if (UseSendKeys) element.SendKeys(Keys.Enter);
                else element.Click();
                return true;
            }
            catch (Exception ex)
            {
                AddLog("TryClickOnElement(): " + ex.Message, true);
                return false;
            }
        }

        #endregion

        #region Find

        private IWebElement TryFindElement(string xPath)
        {
            try
            {
                return wait.Until(e => e.FindElement(By.XPath(xPath)));
            }
            catch (Exception ex)
            {
                AddLog("TryFindElement(" + xPath + "): " + ex.Message, true);
                return null;
            }
        }

        private ReadOnlyCollection<IWebElement> TryFindElements(string xPath)
        {
            try
            {
                return wait.Until(e => e.FindElements(By.XPath(xPath)));
            }
            catch (Exception ex)
            {
                AddLog("TryFindElements(" + xPath + "): " + ex.Message, true);
                return null;
            }
        }

        private IAlert TryFindAlert()
        {
            try
            {
                return wait.Until(ExpectedConditions.AlertIsPresent());
            }
            catch (Exception ex)
            {
                AddLog("TryFindAlert(): " + ex.Message, true);
                return null;
            }
        }

        #endregion

        #endregion

        public bool CheckSignatureMETS(string auName = "Хамидулин Илья Хамитович")
        {
            
            try
            {
                // 1 Переходим на страницу
                driver.Navigate().GoToUrl("https://m-ets.ru/signTest");

                // 2 Кликаем по кнопке
                if (!TryClickOnElement("//button[@id='submitbtn']")) return false;

                // 3 Получаем элементы из выпадающего списка
                var certificates = TryFindElements("//select[@id='certSel']/option");
                if (certificates == null) return false;

                // 4 Кликаем по выпадающему списку
                if (!TryClickOnElement("//select[@id='certSel']")) return false;

                // 5 Кликаем на нужный элемент
                foreach (var cert in certificates)
                {
                    if (cert.Text.Contains(auName))
                    {
                        if (!TryClickOnElement(cert)) return false;
                        break;
                    }   
                }

                // 6 Кнопка OK
                if (!TryClickOnElement("/html/body/div[20]/div[11]/div/button[1]")) return false;

                // 7 Получаем уведомление (Если оно есть)
                IAlert alert = TryFindAlert();
                if (alert != null)
                {
                    // проверяем содержимое Alert-а
                    if (alert != null && !alert.Text.Contains("Этот веб-сайт пытается выполнить операцию с ключами или сертификатами от имени пользователя."))
                    {
                        AddLog("Неверный Alert: " + alert.Text);
                        alert.Accept();
                        return false;
                    }
                    alert.Accept();
                }

                // 8 Проверяем сообщение об успешном результате
                var el = TryFindElement("/html/body/div[13]/div[3]/div[2]/div/div[5]/p");
                if (el == null || !el.Text.Contains(auName)) return false;
                else return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}


#region _
#endregion