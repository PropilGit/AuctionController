using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using AuctionController.Models;


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

        public SeleniumController(int waitTime = 10)
        {

            //Firefox
            string path = "6r6tb0tm.mets";//"C:\\Users\\Propil\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles\\6r6tb0tm.mets";

            FirefoxProfileManager pm = new FirefoxProfileManager();
            pm.GetProfile("m-ets");

            FirefoxOptions options = new FirefoxOptions();
            options.Profile = new FirefoxProfile(path);

            CodePagesEncodingProvider.Instance.GetEncoding(437);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            driver = new FirefoxDriver(options);
            //driver = new FirefoxDriver();

            // IE
            //driver = new InternetExplorerDriver();
            UpdateWebDriverWait(waitTime);
        }
        ~SeleniumController()
        {
            driver.Close();
        }

        #region Base

        public void UpdateWebDriverWait(int seconds)
        {
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
        }

        #endregion

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
        private bool TryClickOnElement(string xPath, bool useSendKeys = true)
        {
            try
            {
                if (UseSendKeys && useSendKeys) wait.Until(e => e.FindElement(By.XPath(xPath))).SendKeys(Keys.Enter);
                else wait.Until(e => e.FindElement(By.XPath(xPath))).Click();
                return true;
            }
            catch (Exception ex)
            {
                AddLog("TryClickOnElement(" + xPath + "): " + ex.Message, true);
                return false;
            }
        }
        private bool TryClickOnElement(IWebElement element, bool useSendKeys = true)
        {
            try
            {
                if (UseSendKeys && useSendKeys) element.SendKeys(Keys.Enter);
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

        private IWebElement TryFindElement(IWebElement parent, string xPath)
        {
            try
            {
                return wait.Until(parent => parent.FindElement(By.XPath(xPath)));
            }
            catch (Exception ex)
            {
                AddLog("TryFindElement(IWebElement, " + xPath + "): " + ex.Message, true);
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

        #region Commands

        public bool CheckSignature_METS_IE(string auName)
        {

            try
            {
                // 1 Переходим на страницу
                driver.Navigate().GoToUrl("https://m-ets.ru/signTest");

                // 2 Кликаем по кнопке
                if (!TryClickOnElement("//button[@id='submitbtn']")) return false;

                // 3 Получаем элементы списка
                // /html/body/div[2]/div/div/div[2]/div[2]/label[1]/div
                var certificates = TryFindElements("//div[@id='certSel2']/label");
                if (certificates == null) return false;

                // (4) Кликаем по выпадающему списку
                //if (!TryClickOnElement("//select[@id='certSel']")) return false;
                
                // 5 Кликаем по нужному элементу
                // 5.1 Перебираем элементы списка
                foreach (var cert in certificates)
                {
                    // 5.2 Получаем span с заголовком
                    var c = TryFindElement(cert, "//div/div[1]/span");
                    if (c == null) return false;

                    // 5.3 Кликаем по элементу если его заголовок содержит имя АУшника
                    if (c.Text.Contains(auName))
                    {
                        if (!TryClickOnElement(cert, false)) return false;
                        break;
                    }
                }
                
                // 6 Кнопка Далее
                if (!TryClickOnElement("//input[@id='button_cert-dalee']")) return false; // "/html/body/div[20]/div[11]/div/button[1]"

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
                var el = TryFindElement("//div[@id='content']/div[1]/div[@class='sign-sert']/p");
                if (el == null || !el.Text.Contains(auName)) return false;
                else return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool CheckSignature_METS_MF(string auName)
        {

            try
            {
                // 1 Переходим на страницу
                driver.Navigate().GoToUrl("https://m-ets.ru/signTest");

                // 2 Кликаем по кнопке
                if (!TryClickOnElement("//*[@id='submitbtn']")) return false;

                // 3 Получаем уведомление (Если оно есть)
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

                // 3 Получаем элементы списка
                var certificates = TryFindElements("//div[@id='certSel2_listCert']/label/div");
                if (certificates == null) return false;

                // 5 Кликаем по нужному элементу
                // 5.1 Перебираем элементы списка
                foreach (var cert in certificates)
                {
                    // 5.2 Получаем span с заголовком
                    var c = TryFindElement(cert, "//div[1]/span");
                    if (c == null) return false;

                    // 5.3 Кликаем по элементу если его заголовок содержит имя АУшника
                    if (c.Text.Contains(auName))
                    {
                        if (!TryClickOnElement(cert)) return false;
                        break;
                    }
                }

                // 6 Кнопка Далее
                if (!TryClickOnElement("//input[@id='button_cert-dalee']")) return false;

                // 8 Проверяем сообщение об успешном результате
                var el = TryFindElement("//div[@id='content']/div[1]/div[@class='sign-sert']/p");
                if (el == null || !el.Text.Contains(auName)) return false;
                else return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Login_METS_IE(ArbitralManager AU)
        {
            try
            {
                // 0 Проверка (Может быть уже залогинились)
                var success = TryFindElement("//div[@id='header-right']/div[@id='login-form-logged']/div[3]");
                if (success != null && success.Text.Contains(AU.Name)) return true;

                // 1 Переходим на стрницу
                driver.Navigate().GoToUrl(@"https://m-ets.ru/");

                // 2 Кликаем по кнопке "Личный кабинет"
                if (!TryClickOnElement("//*[@id='header-right']/div[2]/button")) return false;

                // 3 Вводим Логин
                var login = TryFindElement("//*[@id='postAuthDialog']/div[2]/form/div[1]/div[2]/input");
                if (login == null) return false;
                login.SendKeys(AU.Login);

                // 4 Вводим Пароль
                var pass = TryFindElement("//*[@id='postAuthDialog']/div[2]/form/div[2]/div[2]/input");
                if (pass == null) return false;
                pass.SendKeys(AU.Password);

                // 5 Кликаем по кнопке "Войти"
                if (!TryClickOnElement("//*[@id='postAuthDialog']/div[2]/form/input[2]")) return false;

                // 6 Проверка успеха
                success = TryFindElement("//*[@id='login-form-logged']");
                if (success == null || !success.Text.Contains(AU.Name)) return false;
                else return true;

            }
            catch (Exception ex)
            {
                AddLog(ex.Message, true);
                return false;
            }
        }
        public bool Login_METS_MF(ArbitralManager AU)
        {
            try
            {

                // 1 Переходим на стрницу
                driver.Navigate().GoToUrl(@"https://m-ets.ru/");

                // 2 Кликаем по кнопке "Личный кабинет"
                if (!TryClickOnElement("//div[@id='header-right']/div[2]/button")) return false;

                // 3 Вводим Логин
                var login = TryFindElement("//*[@id='postAuthDialog']/div[2]/form/div[1]/div[2]/input");
                if (login == null) return false;
                login.SendKeys(AU.Login);

                // 4 Вводим Пароль
                var pass = TryFindElement("//*[@id='postAuthDialog']/div[2]/form/div[2]/div[2]/input");
                if (pass == null) return false;
                pass.SendKeys(AU.Password);

                // 5 Кликаем по кнопке "Войти"
                if (!TryClickOnElement("//*[@id='postAuthDialog']/div[2]/form/input[2]")) return false;

                // 6 Проверка успеха
                var success = TryFindElement("//*[@id='login-form-logged']/div[1]");
                if (success == null || !success.Text.Contains(AU.Name)) return false;
                else return true;

            }
            catch (Exception ex)
            {
                AddLog(ex.Message, true);
                return false;
            }
        }

        #region ParseLots

        public ObservableCollection<Lot> ParseLots_AURU(List<int> lotIds)
        {
            try
            {
                ObservableCollection<Lot> result = new ObservableCollection<Lot>();
                foreach (var id in lotIds)
                {
                    result.Add(ParseLot_AURU(id));
                }
                return result;
            }
            catch (Exception ex)
            {
                AddLog("ParseLots(List<int>): " + ex.Message);
                return null;
            }
        }

        public Lot ParseLot_AURU(int lotId)
        {
            try
            {
                // 1 Переходим на страницу лота
                driver.Navigate().GoToUrl(@"https://nsk.au.ru/" + lotId);

                // 2 Number
                int number = 0;
                var numberEl = TryFindElement("//*[@id='item-page_0']/div/div/div[1]/div[3]/div[1]/div[2]/div/span[5]/a");
                if (numberEl != null && numberEl.Text != "") number = Int32.Parse(numberEl.Text);
                else return null;

                // 3 Name
                string name = "";
                var nameEl = TryFindElement("//*[@id='item-page_0']/div/div/div[1]/div[3]/div[1]/div[1]/h1");
                if (nameEl != null && nameEl.Text != "") name = nameEl.Text;
                else return null;

                // 4 CurrentPrice
                double currentPrice = 0;
                var currentPriceEl = TryFindElement("//*[@id='trading']/div/div[1]/div/div[2]/div[2]/div[1]/div[1]/span/span[1]");
                if (currentPriceEl != null && currentPriceEl.Text != "") currentPrice = Double.Parse(currentPriceEl.Text);
                else return null;

                // 5 StartDate
                DateTime startDate = DateTime.UnixEpoch;
                var startDateEl = TryFindElement("//*[@id='trading']/div/div[5]/div/div[2]/div/span[2]/span[2]");
                if (startDateEl != null && startDateEl.Text != "") startDate = DateTime.Parse(startDateEl.Text);
                else return null;

                // 6
                return new Lot(lotId, number, name, currentPrice, startDate);
            
            }
            catch (Exception ex)
            {
                AddLog("ParseLot_AURU(" + lotId + "): " + ex.Message);
                return null;
            }
        }

        #endregion

        #endregion
    }
}


#region _
#endregion