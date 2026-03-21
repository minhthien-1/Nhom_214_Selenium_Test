using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Nhom_214.Pages
{
    public class LoginPage
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        private IWebElement EmailInput => _driver.FindElement(By.Id("email"));
        private IWebElement PassInput => _driver.FindElement(By.Id("password"));
        // Sửa lại cách tìm nút Login theo HTML bạn gửi (nút có name='login')
        private IWebElement LoginBtn => _driver.FindElement(By.CssSelector("button[name='login']"));

        public void Login(string email, string pass)
        {
            EmailInput.Clear(); EmailInput.SendKeys(email); Thread.Sleep(300);
            PassInput.Clear(); PassInput.SendKeys(pass); Thread.Sleep(300);
            LoginBtn.Click();
        }

        // HÀM QUAN TRỌNG: Xử lý SweetAlert2
        public string HandleSweetAlert()
        {
            try
            {
                // 1. Chờ cho đến khi khung thông báo của SweetAlert xuất hiện
                _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("swal2-popup")));

                // 2. Lấy nội dung text của thông báo (cả tiêu đề và nội dung phụ)
                string title = _driver.FindElement(By.ClassName("swal2-title")).Text;
                string text = "";
                try { text = _driver.FindElement(By.ClassName("swal2-html-container")).Text; } catch { }

                // 3. Tìm nút "OK" của SweetAlert (class mặc định là .swal2-confirm)
                IWebElement okBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("swal2-confirm")));

                // 4. Bấm OK bằng JavaScript để đảm bảo không bị lỗi che khuất
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("arguments[0].click();", okBtn);

                // 5. Đợi bảng thông báo biến mất hẳn mới trả về kết quả
                _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("swal2-container")));

                return title + " " + text;
            }
            catch (WebDriverTimeoutException)
            {
                return "Không có SweetAlert";
            }
        }

        // Xử lý lỗi "Please fill out this field" (HTML5 Validation)
        public string GetHtml5ValidationMessage(string fieldId)
        {
            IWebElement input = _driver.FindElement(By.Id(fieldId));
            return input.GetAttribute("validationMessage");
        }
    }
}