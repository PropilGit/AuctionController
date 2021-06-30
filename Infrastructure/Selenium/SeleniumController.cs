using OpenQA.Selenium;
using OpenQA.Selenium.IE;
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

        public string CheckSignature_EFRSB(string auName = "Ляхов Андрей Николаевич")
        {
            
            try
            {
                driver.Navigate().GoToUrl("https://bankrot.fedresurs.ru/CheckSignature.aspx");
                wait.Until(e => e.FindElement(By.Id("ctl00_cphBody_ds1_ibCheck"))).Click();

                var Certificates = wait.Until(e => e.FindElements(By.XPath("//div[@class='certificate-list']/a")));
                foreach (var c in Certificates)
                {
                    string subject = c.FindElement(By.ClassName("subject")).Text;
                    if (subject == auName)
                    {
                        c.Click();
                        c.FindElement(By.XPath("//button[contains(@data-element-type, 'select-button')]")).Click();
                        break;
                    }
                }
                IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());
                if (alert.Text == "Электронная подпись успешно прошла проверку.")
                {
                    alert.Accept();
                    return "OK";
                }
                else return "FAIL: " + alert.Text;
                return "ERROR: На этапе получения Alert";
            }
            catch (Exception ex)
            {
                return "ERROR: Исключение:" + ex.Message;
            }
        }
    }
}
