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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

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

        public SeleniumController(int waitTime = 5)
        {

            //Firefox
            string path = @"m-ets";

            FirefoxOptions options = new FirefoxOptions();
            options.Profile = new FirefoxProfile(path);
            //options.Profile.EnableNativeEvents = false;
            options.PageLoadStrategy = PageLoadStrategy.Eager;

            CodePagesEncodingProvider.Instance.GetEncoding(437);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            driver = new FirefoxDriver(options);

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
        private IWebElement TryFindElement_Wait(string xPath, int attemptsCount = 5, int delay = 1000)
        {
            try
            {
                int i = 0;
                while (i++ < attemptsCount)
                {
                    try
                    {
                        return driver.FindElement(By.XPath(xPath));
                    }
                    catch (StaleElementReferenceException e)
                    {
                        Thread.Sleep(delay);
                        continue;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                AddLog("TryFindElement_Wait(" + xPath + "): " + ex.Message, true);
                return null;
            }
        }
        private IWebElement TryFindElementByCSS(string cssSel)
        {
            try
            {
                return wait.Until(e => e.FindElement(By.CssSelector(cssSel)));
            }
            catch (Exception ex)
            {
                AddLog("TryFindElementByCSS(" + cssSel + "): " + ex.Message, true);
                return null;
            }
        }

        private IWebElement TryFindElement(IWebElement el, string xPath)
        {
            try
            {
                return el.FindElement(By.XPath(xPath));
            }
            catch (Exception ex)
            {
                AddLog("TryFindElement(IWebElement, " + xPath + "): " + ex.Message, true);
                return null;
            }
        }
        private IWebElement TryFindElementByCSS(IWebElement el, string cssSel)
        {
            try
            {
                return el.FindElement(By.CssSelector(cssSel));
            }
            catch (Exception ex)
            {
                AddLog("TryFindElement(IWebElement, " + cssSel + "): " + ex.Message, true);
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
                bool test = wait.Until(ExpectedConditions.TitleContains("Подтверждение доступа"));
                IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());
                return alert;
            }
            catch (Exception ex)
            {
                AddLog("TryFindAlert(): " + ex.Message, true);
                return null;
            }
        }

        private IAlert WaitForAlert(int attemptsCount = 5, int delay = 1000)
        {
            int i = 0;
            while (i++ < attemptsCount)
            {
                try
                {
                    return driver.SwitchTo().Alert();
                }
                catch (NoAlertPresentException e)
                {
                    Thread.Sleep(delay);
                    continue;
                }
            }
            return null;
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

                // 3 Получаем элементы списка
                var certificates = TryFindElements("//div[@id='certSel2_listCert']/label/div");
                if (certificates == null) return false;

                // 5 Кликаем по нужному элементу
                // 5.1 Перебираем элементы списка
                foreach (var cert in certificates)
                {
                    // 5.2 Получаем span с заголовком
                    //var c = TryFindElement(cert, "//div[1]/span");
                    //if (c == null) return false;

                    // 5.3 Кликаем по элементу если его заголовок содержит имя АУшника
                    if (cert.Text.Contains(auName))
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
        public bool Login_METS_IE(Bidder AU)
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
        public bool Login_METS_MF(Bidder AU)
        {
            try
            {
                // 1 Переходим на стрницу
                driver.Navigate().GoToUrl(@"https://m-ets.ru/");

                // 2 Проверяем залогинены ли мы сейчас 
                var success = TryFindElement("//*[@id='login-form-logged']/div[3]");
                if (success != null)
                {
                    if(success.Text.Contains(AU.Name)) return true;
                }

                // 3 Кликаем по кнопке "Личный кабинет"
                if (!TryClickOnElement("//div[@id='header-right']/div[2]/button")) return false;

                // 4 Вводим Логин
                var login = TryFindElement("//*[@id='postAuthDialog']/div[2]/form/div[1]/div[2]/input");
                if (login == null) return false;
                login.SendKeys(AU.Login);

                // 5 Вводим Пароль
                var pass = TryFindElement("//*[@id='postAuthDialog']/div[2]/form/div[2]/div[2]/input");
                if (pass == null) return false;
                pass.SendKeys(AU.Password);

                // 6 Кликаем по кнопке "Войти"
                if (!TryClickOnElement("//*[@id='postAuthDialog']/div[2]/form/input[2]")) return false;

                // 7 Проверка успеха
                success = TryFindElement("//*[@id='login-form-logged']/div[3]");
                if (success == null || !success.Text.Contains(AU.Name)) return false;
                else return true;

            }
            catch (Exception ex)
            {
                AddLog(ex.Message, true);
                return false;
            }
        }

        #endregion

        #region ParseLots

        public ObservableCollection<Lot> ParseLots()
        {
            try
            {
                ObservableCollection<Lot> result = new ObservableCollection<Lot>();
                foreach (var id in TestLotIds_AURU)
                {
                    result.Add(ParseLot_AURU_MF(id));
                }
                return result;
            }
            catch (Exception ex)
            {
                AddLog("ParseLots(List<int>): " + ex.Message);
                return null;
            }
        }

        #region AURU

        List<string> TestLotIds_AURU = new List<string>()
        {
            "17841129",
            "17841270",
            "17011962",
            "13748863",
            "13742798",
            "7725899",
            "17731701",
            "17843855",
            "17843854",
            "15661860"
        };
        public Lot ParseLot_AURU_MF(string lotId)
        {
            try
            {
                // 1 Переходим на страницу лота
                driver.Navigate().GoToUrl(@"https://nsk.au.ru/" + lotId);

                // /html/body/div[1]/div[4]/div[1]/div/div/div/div[2]/div/a/div
                TryClickOnElement("/html/body/div[1]/div[4]/div[1]/div/div/div/div[2]/div/a/div");

                // 2 Number
                int number = 0;
                var numberEl = TryFindElement("//*[@id='item-page_0']/div/div/div[1]/div[3]/div[1]/div[2]/div/span[5]/a");
                if (numberEl != null && numberEl.Text != "") number = Int32.Parse(lotId);
                else return Lot.Error(lotId);

                // 3 Name
                string name = "";
                var nameEl = TryFindElement("/html/body/div[1]/div[3]/div[1]/div/div/div[1]/div[3]/div[1]/div[1]/h1");
                if (nameEl != null && nameEl.Text != "") name = nameEl.Text;
                else return Lot.Error(lotId);

                // 4 CurrentPrice
                float currentPrice = 0;
                //                                   
                var currentPriceEl = TryFindElement("/html/body/div[1]/div[3]/div[1]/div/div/div[1]/div[3]/div[3]/div[1]/div/div[1]/div/div[2]/div[2]/div[1]/div[1]/span/span[1]");
                if (currentPriceEl != null && currentPriceEl.Text != "") currentPrice = float.Parse(currentPriceEl.Text);
                else return Lot.Error(lotId);

                // 5 StartDate
                DateTime startDate = DateTime.UnixEpoch;
                var startDateEl = TryFindElement("/html/body/div[1]/div[3]/div[1]/div/div/div[1]/div[3]/div[3]/div[1]/div/div[4]/div/div[2]/div/span[2]/span[2]");
                if (startDateEl != null && startDateEl.Text != "") startDate = ParseDate(startDateEl.Text);
                else return Lot.Error(lotId);

                // 6
                return new Lot(lotId.ToString(), number, name, currentPrice);

            }
            catch (Exception ex)
            {
                AddLog("ParseLot_AURU(" + lotId + "): " + ex.Message);
                return Lot.Error(lotId);
            }
        }
        DateTime ParseDate(string text)
        {
            //(12 июля 2021 09:59)
            Regex regex = new Regex(@"[0-9]{2}[ ][а-я]+[ ][0-9]{4}");
            MatchCollection matches = regex.Matches(text);
            if (matches.Count == 1)
            {
                string match = matches[0].Value;
                string day = match.Substring(0, 2);

                string mounth = match.Substring(2, (match.Length - 2 - 4));
                if (mounth.Contains("янв")) mounth = "01";
                else if (mounth.Contains("фев")) mounth = "02";
                else if (mounth.Contains("мар")) mounth = "03";
                else if (mounth.Contains("апр")) mounth = "04";
                else if (mounth.Contains("май") || mounth.Contains("мая")) mounth = "05";
                else if (mounth.Contains("июн")) mounth = "06";
                else if (mounth.Contains("июл")) mounth = "07";
                else if (mounth.Contains("авг")) mounth = "08";
                else if (mounth.Contains("сен")) mounth = "09";
                else if (mounth.Contains("окт")) mounth = "10";
                else if (mounth.Contains("ноя")) mounth = "11";
                else if (mounth.Contains("дек")) mounth = "12";
                else return default;
                string year = match.Substring(match.Length - 4, 4);

                return DateTime.Parse(day + "." + mounth + "." + year + " 13:00");
            }
            else return default;
        }

        #endregion

        public ObservableCollection<Lot> ___ParseLots_METS_TEST_MF()
        {
            try
            {
                // 1 
                driver.Navigate().GoToUrl(@"https://m-ets.ru/generalView?id=174687291");

                // 2
                var lots = TryFindElements("//*[contains(@id,'block_lot_')]");

                //3
                ObservableCollection<Lot> result = new ObservableCollection<Lot>();
                foreach (var lot in lots)
                {
                    string id = lot.GetAttribute("id").Substring(10);
                    int number = Int32.Parse(TryFindElement(lot, "table[1]/tbody/tr/th").Text.Substring(18));
                    string name = TryFindElement(lot, "table[2]/tbody/tr[3]/td[2]").Text;
                    float currentPrice = float.Parse(TryFindElement(lot, "table[2]/tbody/tr[9]/td[2]/table/tbody/tr[1]/td[4]").Text);
                    //DateTime startDate = DateTime.Parse(TryFindElement(lot, "table[2]/tbody/tr[9]/td[2]/table/tbody/tr[1]/td[2]/span").Text);

                    result.Add(new Lot(id, number, name, currentPrice));
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ObservableCollection<Lot> ParseAllLotsOnPage_METS_MF(string aucId)
        {
            ObservableCollection<Lot> result = new ObservableCollection<Lot>();
            try
            {
                // 1 
                driver.Navigate().GoToUrl(@"https://m-ets.ru/generalView?id=" + aucId);

                // 2
                IEnumerable<IWebElement> lots = TryFindElements("//*[contains(@id,'block_lot_')]");
                if(lots == null)
                {
                    AddLog("ParseAllLotsOnPage_METS_MF(): Не найдено ни одного лота!");
                    return new ObservableCollection<Lot>();
                }

                //3
                result = new ObservableCollection<Lot>();
                //Parallel.ForEach<IWebElement>(lots, lot => result.Add(ParseLot_METS_MF(lot)));
                //
                foreach (var lot in lots)
                {
                    result.Add(___ParseLot_METS_MF_after(lot));
                }
                return result;
                //
            }
            catch (Exception ex)
            {
                AddLog("ParseAllLotsOnPage_METS_MF(): " + ex.Message);
                return result;
            }
        }

        /*
        public Lot ParseLot_METS_MF(string id)
        {
            try
            {
                // 1 
                driver.Navigate().GoToUrl(@"https://m-ets.ru/generalView?id=174687291");

                // 2
                string xpath = "//*[@id='block_lot_" + id + "']";
                IWebElement lot = TryFindElement(xpath);

                // 3
                return ParseLot_METS_MF(lot);
            }
            catch (Exception ex)
            {
                AddLog("ParseLot_AURU(): " + ex.Message);
                return Lot.Error("");
            }
        }
        */
        // После торгов
        public Lot ___ParseLot_METS_MF_after(IWebElement lot)
        {
            try
            {
                string id = lot.GetAttribute("id").Substring(10);
                int number = Int32.Parse(TryFindElement(lot, "table[1]/tbody/tr/th").Text.Substring(18));
                string name = TryFindElement(lot, "table[2]/tbody/tr[3]/td[2]").Text;
                //float currentPrice = float.Parse(TryFindElement(lot, "table[2]/tbody/tr[9]/td[2]/table/tbody/tr[1]/td[4]").Text);
                string currentPriceString = TryFindElement(lot, "table[2]/tbody/tr[9]/td[2]/span").Text;
                float currentPrice = float.Parse(currentPriceString.Substring(0, currentPriceString.Length - 5));
                
                //DateTime startDate = DateTime.Parse(TryFindElement(lot, "table[2]/tbody/tr[9]/td[2]/table/tbody/tr[1]/td[2]/span").Text);

                return new Lot(id, number, name, currentPrice);
            }
            catch (Exception ex)
            {
                AddLog("ParseLot_AURU(): " + ex.Message);
                return Lot.Error("");
            }
        }
        public Lot ParseLot_METS_MF(IWebElement lot)
        {
            try
            {
                string id = lot.GetAttribute("id").Substring(10);

                int number = Int32.Parse(TryFindElement(lot, "table[1]/tbody/tr/th").Text.Substring(18));

                string name = TryFindElement(lot, "table[2]/tbody/tr[4]/td[2]").Text;

                string _startPrice = TryFindElement(lot, "table[2]/tbody/tr[10]/td[2]/span").Text;
                float startPrice = float.Parse(_startPrice.Substring(0, _startPrice.Length - 5));

                //DateTime startDate = DateTime.Parse(TryFindElement(lot, "table[2]/tbody/tr[9]/td[2]/table/tbody/tr[1]/td[2]/span").Text);

                return new Lot(id, number, name, startPrice);
            }
            catch (Exception ex)
            {
                AddLog("ParseLot_METS_MF(): " + ex.Message);
                return Lot.Error("");
            }
        }
        public Lot ParseSingleLot_METS_MF(string aucId, string lotId)
        {
            try
            {
                // 1 
                driver.Navigate().GoToUrl(@"https://m-ets.ru/generalView?id=" + aucId);

                // 2
                IWebElement lot = TryFindElement("//*[contains(@id,'block_lot_" + lotId + "')]");

                //3
                return ParseLot_METS_MF(lot);
            }
            catch (Exception ex)
            {
                AddLog("ParseSingleLot_METS_MF(): " + ex.Message);
                return Lot.Error(lotId);
            }
        }

        public bool UpdateLot_METS_MF(Lot lot)
        {
            try
            {
                // 1
                driver.Navigate().GoToUrl(@"https://m-ets.ru/auctionBid?lot_id=" + lot.Id);

                string betsTable = TryFindElement("//div[@id='logtable']").GetAttribute("innerHTML");

                // 2 Текущая цена
                float current_price = float.Parse(TryFindElement("//*[@id='current_price']").Text);
                // Если цена не изменилась ничего не делаем
                //if (lot.CurrentPrice == current_price) return false;

                // 3 Время завершения торгов
                DateTime trade_end_date = DateTime.Parse(TryFindElement("//*[@id='trade_end_date']").Text);

                //lot.Update(trade_end_date, current_price);

                
                // 4 Ставки
                string[] betsRows = betsTable.Split("<tr>");

                Bet[] lastBets = new Bet[4];
                for (int i = 0; i < lastBets.Length; i++)
                {
                    string row = betsRows[betsRows.Length - 1 - i];
                    row = row.Replace("&nbsp;", "");
                    //string str_time = TryFindElement("/td[1]").Text;
                    //string str_time = TryFindElement_Wait(10, 2,"//div[@id='logtable']/table/tbody/tr[" + (betsCount - i) + "]/td[1]").Text;
                    //int startIndex = betsRows[betsRows.Length - 1 - i].IndexOf(discriptionTag) + di;
                    string str_time = FindRegExp(row, @"[0-9]{2}[:][0-9]{2}[:][0-9]{2}");//betsRows[betsRows.Length - 1 - i].Substring(27, 20);   
                    DateTime time = DateTime.Parse(str_time);

                    //string str_discription = TryFindElement("/td[2]").Text;
                    //string str_discription = TryFindElement_Wait(10, 2, "//div[@id='logtable']/table/tbody/tr[" + (betsCount - i) + "]/td[2]").Text;
                    string str_discription = FindRegExp(row, @"<td>[ a-я\d\s\,]+<\/td>"); //<td> Принято ценовое предложение на сумму 2&nbsp;475,00  </td>
                    if (str_discription == "") str_discription = FindRegExp(row, @"<b>[ a-я\d\s\/b\,]+<\/b>");
                    string str_summ = FindRegExp(str_discription, @"[0-9 \,]{2,15}");
                    //if (str_summ == "") return;
                    float summ = float.Parse(str_summ);

                    bool isMine = false;
                    if (str_discription.Contains("Ваше")) isMine = true;
                    lastBets[i] = new Bet(summ, "", time, isMine);
                }

                // 5 Обновляем данные в лоте
                return lot.Update(trade_end_date, current_price, lastBets);

                
            }
            catch (Exception ex)
            {
                AddLog("UpdateLot_METS_MF(lot:" + lot.Id + ")" + ex.Message, true);
                return false;
            }
        }
        public bool MakeBet_METS_MF(Lot lot, Bidder bidder)
        {
            try
            {
                // 1
                driver.Navigate().GoToUrl(@"https://m-ets.ru/auctionBid?lot_id=" + lot.Id);

                // 2 Проверяем текущую цену
                float current_price = float.Parse(TryFindElement("//*[@id='current_price']").Text);
                if (current_price != lot.CurrentPrice) return true;

                // 3
                TryFindElement("//input[@id='send_btn']").Click();

                // 4
                IAlert alert = WaitForAlert();//TryFindAlert();
                if (alert != null)
                {
                    // проверяем содержимое Alert-а
                    if (alert != null && !alert.Text.Contains("Подписать и отправить ценовое предложение"))
                    {
                        AddLog("Неверный Alert: " + alert.Text, true);
                        alert.Accept();
                        return false;
                    }
                    alert.Accept();
                }
                else return false;

                // 5 Получаем элементы списка
                var certificates = TryFindElements("//div[@id='certSel2']/div/label/div");
                if (certificates == null) return false;

                // 6 Кликаем по нужному элементу
                // 6.1 Перебираем элементы списка
                foreach (var cert in certificates)
                {
                    // 6.2 Кликаем по элементу если его заголовок содержит имя АУшника
                    if (cert.Text.Contains(bidder.Name))
                    {
                        if (!TryClickOnElement(cert)) return false;
                        break;
                    }
                }

                // 7 Кнопка Далее
                if (!TryClickOnElement("//input[@id='button_cert-dalee']")) return false;

                // 8
                alert = TryFindAlert();
                if (alert != null)
                {
                    // проверяем содержимое Alert-а
                    if (alert != null && !alert.Text.Contains("Ваше ценовое предложение на сумму"))
                    {
                        AddLog("Неверный Alert: " + alert.Text, true);
                        alert.Accept();
                        return false;
                    }
                    alert.Accept();
                }
                else return false;
                
                return true;
            }
            catch (Exception ex)
            {
                AddLog("MakeBet_METS_MF: " + ex.Message);
                return false;
            }
        }

        #endregion

        string FindRegExp(string text, string regExp)
        {
            Regex regex = new Regex(regExp);
            MatchCollection matches = regex.Matches(text);
            if (matches.Count > 0)
            {
                return matches[0].Value;
            }
            return "";
        }

    }
}


#region _
#endregion