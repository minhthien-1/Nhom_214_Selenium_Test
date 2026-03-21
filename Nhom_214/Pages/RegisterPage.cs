using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI; // Cần thêm
using SeleniumExtras.WaitHelpers; // Cần thêm

namespace Nhom_214.Pages
{
    public class RegisterPage
    {
        private IWebDriver _driver;
        public RegisterPage(IWebDriver driver) => _driver = driver;

        // Định danh các ô nhập liệu theo đúng ID trong code JS của bạn
        private IWebElement NameInput => _driver.FindElement(By.Id("fullName"));
        private IWebElement UsernameInput => _driver.FindElement(By.Id("Uname"));
        private IWebElement EmailInput => _driver.FindElement(By.Id("email"));
        private IWebElement PhoneInput => _driver.FindElement(By.Id("phone"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
        private IWebElement ConfirmPassInput => _driver.FindElement(By.Id("confirmPassword"));
        private IWebElement RegisterBtn => _driver.FindElement(By.CssSelector("button[type='submit']"));

        public void FillForm(string name, string user, string email, string phone, string pass, string confirm)
        {
            // Nhập liệu có nghỉ nhẹ để bạn quan sát tool chạy
            NameInput.Clear(); NameInput.SendKeys(name); Thread.Sleep(200);
            UsernameInput.Clear(); UsernameInput.SendKeys(user); Thread.Sleep(200);
            EmailInput.Clear(); EmailInput.SendKeys(email); Thread.Sleep(200);
            PhoneInput.Clear(); PhoneInput.SendKeys(phone); Thread.Sleep(200);
            PasswordInput.Clear(); PasswordInput.SendKeys(pass); Thread.Sleep(200);
            ConfirmPassInput.Clear(); ConfirmPassInput.SendKeys(confirm); Thread.Sleep(200);

            RegisterBtn.Click();
            Thread.Sleep(800); // Đợi Alert hiện ra hoàn toàn
        }
        public string GetAlertTextWithWait(int timeoutSeconds = 10)
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                // Đợi cho đến khi Alert thực sự xuất hiện trong DOM
                IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());
                string text = alert.Text;
                alert.Accept();
                return text;
            }
            catch (WebDriverTimeoutException)
            {
                return "Không tìm thấy Alert sau " + timeoutSeconds + " giây";
            }
        }

       
    }
}
