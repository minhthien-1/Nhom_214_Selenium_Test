using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhom_214.Pages
{
    public class ProfilePage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public ProfilePage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Các Locators
        private IWebElement EditTab => driver.FindElement(By.CssSelector("button[data-tab='edit']"));
        private IWebElement FullNameInput => driver.FindElement(By.Id("editFullName"));
        private IWebElement EmailInput => driver.FindElement(By.Id("editEmail"));
        private IWebElement SaveBtn => driver.FindElement(By.Id("saveProfileBtn"));

        public void UpdateFullName(string newName)
        {
            EditTab.Click();
            FullNameInput.Clear();
            FullNameInput.SendKeys(newName);
            SaveBtn.Click();
        }

        public void UpdateEmail(string newEmail)
        {
            EditTab.Click();
            EmailInput.Clear();
            EmailInput.SendKeys(newEmail);
            SaveBtn.Click();
        }
    }
}
