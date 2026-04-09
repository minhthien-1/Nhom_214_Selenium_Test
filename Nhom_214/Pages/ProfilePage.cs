using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

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

        // --- Hàm bổ trợ đợi phần tử xuất hiện ---
        private IWebElement WaitElement(By by)
        {
            return wait.Until(ExpectedConditions.ElementIsVisible(by));
        }

        // --- Locators các Tab ---
        private IWebElement EditTab => WaitElement(By.CssSelector("button[data-tab='edit']"));
        private IWebElement PassTab => WaitElement(By.CssSelector("button[data-tab='password']"));
        private IWebElement DeleteTab => WaitElement(By.CssSelector("button[data-tab='delete']"));

        // --- Locators Form Chỉnh sửa ---
        private IWebElement UsernameInput => WaitElement(By.Id("editUsername"));
        private IWebElement FullNameInput => WaitElement(By.Id("editFullName"));
        private IWebElement EmailInput => WaitElement(By.Id("editEmail"));
        private IWebElement PhoneInput => WaitElement(By.Id("editPhone"));
        private IWebElement SaveProfileBtn => WaitElement(By.CssSelector("form[id='editForm'] button[type='submit']"));
        private IWebElement BackBtn => WaitElement(By.Id("backBtn"));

        // --- Locators Đổi mật khẩu ---
        private IWebElement OldPassInput => WaitElement(By.Id("oldPassword"));
        private IWebElement NewPassInput => WaitElement(By.Id("newPassword"));
        private IWebElement ConfirmPassInput => WaitElement(By.Id("confirmPassword"));
        private IWebElement SavePassBtn => WaitElement(By.CssSelector("form[id = 'passwordForm'] button[type = 'submit']"));

        // --- Locators Xóa tài khoản ---
        private IWebElement DeleteAccBtn => WaitElement(By.Id("deleteAccountBtn"));

        // --- CÁC HÀM NGHIỆP VỤ ---

        public void UpdateInfo(string username = "", string email = "", string phone = "", string fullname = "")
        {
            EditTab.Click();
            System.Threading.Thread.Sleep(500);

            // Chỉ điền Họ tên nếu tham số này không rỗng (dùng để mồi lỗi web)
            if (!string.IsNullOrEmpty(fullname))
            {
                FullNameInput.Clear();
                FullNameInput.SendKeys(fullname);
            }

            // CHỖ NÀY QUAN TRỌNG: Chỉ khi nào mình truyền số điện thoại mới vào 
            // thì nó mới xóa và nhập. Nếu để rỗng thì nó "để yên" như Thiên muốn.
            if (!string.IsNullOrEmpty(phone))
            {
                PhoneInput.Clear();
                PhoneInput.SendKeys(phone);
            }

            if (!string.IsNullOrEmpty(email))
            {
                EmailInput.Clear();
                EmailInput.SendKeys(email);
            }

            // Click Lưu bằng JavaScript
            IWebElement btn = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("form[id='editForm'] button[type='submit']")));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", btn);
        }

        public void ChangePassword(string oldP, string newP, string confirmP)
        {
            PassTab.Click();
            System.Threading.Thread.Sleep(500);
            OldPassInput.SendKeys(oldP ?? "");
            NewPassInput.SendKeys(newP ?? "");
            ConfirmPassInput.SendKeys(confirmP ?? "");
            SavePassBtn.Click();
        }

        public void DeleteAccount()
        {
            DeleteTab.Click();
            System.Threading.Thread.Sleep(500);
            DeleteAccBtn.Click();
        }

        public void ClickBack()
        {
            EditTab.Click();
            System.Threading.Thread.Sleep(300);
            BackBtn.Click();
        }

        public string GetProfileNotification()
        {
            // Đợi tối đa 5 giây, quét mỗi 0.5s
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    // 1. Ưu tiên tìm theo ID mà SelectorsHub vừa báo (#successAlert)
                    var successAlert = driver.FindElements(By.Id("successAlert"));
                    if (successAlert.Count > 0 && successAlert[0].Displayed && !string.IsNullOrEmpty(successAlert[0].Text))
                    {
                        return successAlert[0].Text;
                    }

                    // 2. Dự phòng trường hợp lỗi (thường là dangerAlert hoặc tương tự)
                    var errorAlert = driver.FindElements(By.Id("errorAlert")); // Bạn check xem có ID này không nhé
                    if (errorAlert.Count > 0 && errorAlert[0].Displayed)
                    {
                        return errorAlert[0].Text;
                    }

                    // 3. Dự phòng cho toast-container (nếu có dùng chung)
                    var toast = driver.FindElements(By.Id("toast-container"));
                    if (toast.Count > 0 && !string.IsNullOrEmpty(toast[0].Text))
                    {
                        return toast[0].Text;
                    }
                }
                catch { }

                System.Threading.Thread.Sleep(500);
            }

            return "không có thông báo";
        }
    }
}