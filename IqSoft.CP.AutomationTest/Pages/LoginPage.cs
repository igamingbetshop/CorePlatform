using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.AutomationTest.Pages
{
    public class LoginPage
    {
        IWebDriver Driver { get; }
        IWebElement Username => Driver.FindElement(By.Name("username"));
        IWebElement Password => Driver.FindElement(By.Name("password"));
        IWebElement btnLogin => Driver.FindElement(By.XPath("//input[@value='Log In']"));

        public LoginPage(IWebDriver webDriver)
        {
            Driver = webDriver;
        }

        public void Login(string username, string password)
        {
            Username.SendKeys(username);
            Password.SendKeys(password);
            btnLogin.Submit();
        }



    }
}
