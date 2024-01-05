using OpenQA.Selenium;
namespace IqSoft.CP.AutomationTest.Pages
{
   public class HomePage
    {
        IWebDriver Driver { get; }
        public IWebElement LnkLogin => Driver.FindElement(By.LinkText("Log In"));
        public HomePage(IWebDriver webDriver)
        {
            Driver = webDriver;
        }

        public void ClickLogin() => LnkLogin.Click();
    }
}