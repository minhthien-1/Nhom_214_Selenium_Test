using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhom_214.Pages
{
    public class AdminPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public AdminPage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void UpdateRoomImage(string roomId, string imageName)
        {
            driver.Navigate().GoToUrl("http://localhost:5500/admin/rooms.html");

            // Tìm nút sửa của đúng phòng cần đổi (Giả định roomId nằm trong thuộc tính data)
            var editBtn = driver.FindElement(By.CssSelector($"button[data-room-id='{roomId}']"));
            editBtn.Click();

            var imgInput = driver.FindElement(By.Id("roomImageInput"));
            imgInput.Clear();
            imgInput.SendKeys(imageName);

            driver.FindElement(By.Id("saveRoomBtn")).Click();
        }

        public void CheckCustomerName(string expectedName)
        {
            driver.Navigate().GoToUrl("http://localhost:5500/admin/customers.html");
            var customerTable = driver.FindElement(By.Id("customerTable"));
            // Kiểm tra xem tên có tồn tại trong bảng không
            if (!customerTable.Text.Contains(expectedName))
            {
                throw new Exception($"Không tìm thấy khách hàng: {expectedName}");
            }
        }
    }
}
