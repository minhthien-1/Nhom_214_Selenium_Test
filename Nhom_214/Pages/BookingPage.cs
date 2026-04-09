using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using OpenQA.Selenium.Interactions;

namespace Nhom_214.Pages
{
    public class BookingPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public BookingPage(IWebDriver driver)
        {
            this.driver = driver ?? throw new ArgumentNullException(nameof(driver), "Thien ơi, Driver truyền vào BookingPage bị NULL rồi!");
            this.wait = new WebDriverWait(this.driver, TimeSpan.FromSeconds(15));
        }

        public IWebElement SearchInput => driver.FindElement(By.Id("location"));
        public IWebElement SearchBtn => driver.FindElement(By.Id("searchBtn"));

        public void SearchLocation(string location)
        {
            // Đảm bảo đang ở trang home và đúng port 5500
            if (!driver.Url.Contains("home.html"))
            {
                driver.Navigate().GoToUrl("http://localhost:5500/home.html");
            }

            var searchInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("location")));
            searchInput.Clear();
            Thread.Sleep(500);
            searchInput.SendKeys(location);

            var searchBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("searchBtn")));
            try
            {
                searchBtn.Click();
            }
            catch
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", searchBtn);
            }

            Thread.Sleep(2000);
        }

        public void SelectRoomFromList()
        {
            var firstRoom = wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("room-card")));
            firstRoom.Click();
            Thread.Sleep(2000);
        }

        public void SelectDates(string checkInDate, string checkOutDate)
        {
            PickDateFromCalendar("check-in", checkInDate);
            Thread.Sleep(1000);

            // 1. Đóng lịch Check-in
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("document.body.click();");
            Thread.Sleep(500);

            PickDateFromCalendar("check-out", checkOutDate);

            // 2. Đóng lịch Check-out
            js.ExecuteScript("document.body.click();");
            Thread.Sleep(500);
        }

        private void PickDateFromCalendar(string inputId, string targetDateStr)
        {
            DateTime targetDate = DateTime.ParseExact(targetDateStr, "dd/MM/yyyy", null);
            string targetDay = targetDate.Day.ToString();
            string targetMonth = GetMonthName(targetDate.Month).ToLower();
            string targetYear = targetDate.Year.ToString();

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            var input = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(inputId)));

            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", input);
            Thread.Sleep(500);

            try
            {
                input.Click();
            }
            catch (ElementClickInterceptedException)
            {
                js.ExecuteScript("arguments[0].click();", input);
            }

            Thread.Sleep(1000); // Chờ lịch mở lên hoàn toàn

            bool monthFound = false;
            int safetyBreak = 0;

            while (!monthFound && safetyBreak < 12)
            {
                var monthSelectElement = driver.FindElement(By.CssSelector(".flatpickr-calendar.open .flatpickr-monthDropdown-months"));
                var select = new SelectElement(monthSelectElement);
                string currentMonth = select.SelectedOption.Text.ToLower();

                string currentYear = driver.FindElement(By.CssSelector(".flatpickr-calendar.open .numInput.cur-year")).GetAttribute("value");

                if (currentMonth.Contains(targetMonth) && currentYear == targetYear)
                {
                    monthFound = true;
                }
                else
                {
                    var nextBtn = driver.FindElement(By.CssSelector(".flatpickr-calendar.open .flatpickr-next-month"));
                    nextBtn.Click();
                    Thread.Sleep(500);
                    safetyBreak++;
                }
            }

            // NÂNG CẤP 1: Dùng normalize-space để quét sạch khoảng trắng dư thừa trong HTML
            string xpathDay = $"//div[contains(@class, 'flatpickr-calendar') and contains(@class, 'open')]//span[contains(@class, 'flatpickr-day') " +
                              $"and not(contains(@class, 'prevMonthDay')) " +
                              $"and not(contains(@class, 'nextMonthDay')) " +
                              $"and normalize-space(text())='{targetDay}']";

            // NÂNG CẤP 2: Chờ cho đến khi ngày này thực sự CLICK ĐƯỢC (ElementToBeClickable) thay vì chỉ tồn tại
            var dayElement = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpathDay)));

            try
            {
                // NÂNG CẤP 3: Dùng Actions để mô phỏng rê chuột thật và click (Flatpickr rất thích cách này)
                Actions actions = new Actions(driver);
                actions.MoveToElement(dayElement).Click().Perform();
            }
            catch (Exception)
            {
                js.ExecuteScript("arguments[0].click();", dayElement);
            }

            Thread.Sleep(1000);

            // --- CHỐT CHẶN CUỐI CÙNG ---
            // Kiểm tra xem sau khi click, ô input đã có giá trị chưa. Nếu chưa (do web lag), dùng JS nhét thẳng dữ liệu vào!
            string currentValue = input.GetAttribute("value");
            if (string.IsNullOrEmpty(currentValue))
            {
                js.ExecuteScript($"document.getElementById('{inputId}').value = '{targetDateStr}';");
            }
        }

        private string GetMonthName(int month)
        {
            string[] months = { "", "một", "hai", "ba", "tư", "năm", "sáu", "bảy", "tám", "chín", "mười", "mười một", "mười hai" };
            return months[month];
        }

        public void ClickDatPhong()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("book-now-btn")));

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", btn);

            try
            {
                btn.Click();
            }
            catch
            {
                js.ExecuteScript("arguments[0].click();", btn);
            }
            Thread.Sleep(1500);
        }

        public void ClickThanhToan()
        {
            var payBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("confirm-payment-btn")));
            payBtn.Click();
        }

        public string HandleBrowserAlert()
        {
            try
            {
                IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());
                string alertText = alert.Text;
                alert.Accept();
                return alertText;
            }
            catch (WebDriverTimeoutException)
            {
                return "";
            }
        }
    }
}