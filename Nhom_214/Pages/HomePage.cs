using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhom_214.Pages
{
    public class HomePage
    {
        private IWebDriver _driver;
        public HomePage(IWebDriver driver) => _driver = driver;

        public IWebElement SearchInput => _driver.FindElement(By.Id("location"));
        public IWebElement SearchBtn => _driver.FindElement(By.Id("searchBtn"));
        public IWebElement LogoutBtn => _driver.FindElement(By.Id("logout-btn"));

        public void Search(string location)
        {
            SearchInput.Clear();
            Thread.Sleep(500); // Làm chậm để xem quá trình xóa
            SearchInput.SendKeys(location);
            Thread.Sleep(1000); // Đợi xem text đã nhập chưa
            SearchBtn.Click();

            // Cuộn xuống khu vực danh sách phòng để thấy kết quả
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("window.scrollTo({ top: 1050, behavior: 'smooth' });");
            Thread.Sleep(2000); // Đợi 2 giây để xem kết quả sau khi cuộn
        }
    }
}
