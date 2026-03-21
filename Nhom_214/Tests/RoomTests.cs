using NUnit.Framework;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class RoomTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.CreateDriver();
            driver.Navigate().GoToUrl("http://localhost:5000/home.html");
            driver.Manage().Window.Maximize();
            Thread.Sleep(1000);
        }

        [Test]
        [TestCase("Đà Lạt", true)]    // Mong đợi có phòng
        [TestCase("Nha Trang", false)] // Mong đợi KHÔNG có phòng (Negative Test)
        public void Search_By_Location(string loc, bool isPositive)
        {
            var home = new HomePage(driver);
            home.Search(loc);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var container = wait.Until(d => d.FindElement(By.Id("room-cards-container")));
            string resultText = container.Text.ToLower();

            if (isPositive)
            {
                // Nếu là test có dữ liệu: Không được chứa chữ "không tìm thấy"
                Assert.That(resultText, Does.Not.Contain("không tìm thấy"),
                    $"Lỗi: Đáng lẽ phải có phòng tại {loc}");
                Assert.That(resultText, Does.Contain(loc.ToLower()));
            }
            else
            {
                // SỬA TẠI ĐÂY: Nếu cố tình test dữ liệu trống, phải chứa chữ "không tìm thấy" mới là ĐÚNG (XANH)
                Assert.That(resultText, Does.Contain("không tìm thấy phòng phù hợp"),
                    $"Lỗi: Hệ thống không hiện thông báo lỗi khi tìm {loc}");
                TestContext.WriteLine("Xác nhận: Hệ thống báo lỗi đúng như mong đợi.");
            }

            Thread.Sleep(2000);
        }

        [Test]
        public void Check_Room_Detail_And_Images()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Cuộn xuống để thấy các card phòng
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0, 600);");
            Thread.Sleep(1500);

            // Đợi card phòng hiện ra và click
            var firstRoom = wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("room-card")));
            firstRoom.Click();
            Thread.Sleep(2000);

            // Kiểm tra ảnh chính trong trang chi tiết
            // Lưu ý: Nếu trang chi tiết dùng tag img khác, bạn có thể sửa Selector ở đây
            var img = wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("img")));
            Assert.IsTrue(img.Displayed, "Ảnh phòng phải hiển thị");

            Thread.Sleep(3000); // Dừng lại 3s để check UI trang chi tiết
        }

        [TearDown]
        public void Teardown()
        {
            Thread.Sleep(1000);
            driver?.Quit();
            driver?.Dispose();
        }
    }
}