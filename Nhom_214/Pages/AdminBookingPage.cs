using OpenQA.Selenium;

namespace Nhom_214.Pages
{
    public class AdminBookingPage
    {
        private IWebDriver driver;

        // 1. Locators lấy từ HTML bạn cung cấp
        // Lấy nút Duyệt của đơn hàng ĐẦU TIÊN trên danh sách
        private By btnApproveFirst = By.XPath("(//button[contains(@class, 'btn-approve')])[1]");

        // Lấy nút Hủy của đơn hàng ĐẦU TIÊN trên danh sách
        private By btnRejectFirst = By.XPath("(//button[contains(@class, 'btn-reject')])[1]");

        // Lấy nút Tải lại dựa vào hàm onclick
        private By btnReload = By.XPath("//button[contains(@onclick, 'fetchBookings()')]");

        public AdminBookingPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // 2. Các hàm tương tác
        public void ClickApproveFirstBooking()
        {
            driver.FindElement(btnApproveFirst).Click();
        }

        public void ClickRejectFirstBooking()
        {
            driver.FindElement(btnRejectFirst).Click();
        }

        public void ClickReload()
        {
            driver.FindElement(btnReload).Click();
        }
    }
}