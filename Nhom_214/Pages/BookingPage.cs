using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhom_214.Pages
{
    public class BookingPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public BookingPage(IWebDriver driver)
        {
            this.driver = driver;
            this.driver = driver ?? throw new ArgumentNullException(nameof(driver), "Thien ơi, Driver truyền vào BookingPage bị NULL rồi!");
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        // Đổi ID từ searchInput thành location cho khớp với HTML của bạn
        public IWebElement SearchInput => driver.FindElement(By.Id("location"));
        public IWebElement SearchBtn => driver.FindElement(By.Id("searchBtn"));

        public void SearchLocation(string location)
        {
            // Ép quay lại home một lần nữa cho chắc chắn
            if (!driver.Url.Contains("home.html"))
            {
                driver.Navigate().GoToUrl("http://localhost:5000/home.html");
            }

            // Đợi ô nhập liệu có thể Click được (thay vì chỉ Visible)
            var searchInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("location")));

            searchInput.Clear();
            Thread.Sleep(500);
            searchInput.SendKeys(location);

            // Dùng JavaScript để click nút Search nếu click thường bị che khuất
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
            // Tìm và click vào phòng đầu tiên (Dùng ClassName room-card như file RoomTests)
            var firstRoom = wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("room-card")));
            firstRoom.Click();
            Thread.Sleep(2000); // Chờ vào trang chi tiết
        }

        public void SelectDates(string checkInDate, string checkOutDate)
        {
            // Giả sử định dạng truyền vào là "01/04/2026"
            PickDateFromCalendar("check-in", checkInDate);
            Thread.Sleep(1000); // Nghỉ một chút giữa 2 lần chọn
            PickDateFromCalendar("check-out", checkOutDate);
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
            input.Click();
            Thread.Sleep(1000); // Chờ lịch mở lên hoàn toàn

            bool monthFound = false;
            int safetyBreak = 0;

            while (!monthFound && safetyBreak < 12)
            {
                // QUAN TRỌNG: Thêm ".flatpickr-calendar.open" để chỉ tìm trong cái lịch ĐANG MỞ
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

            // QUAN TRỌNG: Thêm điều kiện contains(@class, 'open') vào XPath
            string xpathDay = $"//div[contains(@class, 'flatpickr-calendar') and contains(@class, 'open')]//span[contains(@class, 'flatpickr-day') " +
                              $"and not(contains(@class, 'prevMonthDay')) " +
                              $"and not(contains(@class, 'nextMonthDay')) " +
                              $"and text()='{targetDay}']";

            var dayElement = wait.Until(ExpectedConditions.ElementExists(By.XPath(xpathDay)));

            // Bọc Try-Catch để nếu Selenium click thường bị trượt do hiệu ứng animate, sẽ dùng JS để "ép" click
            try
            {
                dayElement.Click();
            }
            catch (ElementNotInteractableException)
            {
                js.ExecuteScript("arguments[0].click();", dayElement);
            }

            Thread.Sleep(1000);
        }

        // Hàm hỗ trợ đổi số tháng sang chữ (vì lịch của bạn hiện Tiếng Việt)
        private string GetMonthName(int month)
        {
            string[] months = { "", "một", "hai", "ba", "tư", "năm", "sáu", "bảy", "tám", "chín", "mười", "mười một", "mười hai" };
            return months[month];
        }

        public void ClickDatPhong()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("book-now-btn")));

            // Cuộn tới nút trước khi click
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView(true);", btn);

            try
            {
                btn.Click();
            }
            catch
            {
                // Nếu click thường lỗi, dùng JS click để "ép" nhấn nút
                js.ExecuteScript("arguments[0].click();", btn);
            }
            Thread.Sleep(1500);
        }

        public void ClickThanhToan()
        {
            // Nút thanh toán cuối cùng
            var payBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("confirm-payment-btn")));
            payBtn.Click();
        }
        public string HandleBrowserAlert()
        {
            try
            {
                IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());
                string alertText = alert.Text; // Lưu lại nội dung: "Đặt phòng thành công! Mã..."
                alert.Accept();
                return alertText; // Trả về chuỗi để dùng trong Assert
            }
            catch (WebDriverTimeoutException)
            {
                return "";
            }
        }
    }
}
