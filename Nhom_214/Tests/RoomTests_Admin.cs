using NUnit.Framework;
using OpenQA.Selenium;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using System;
using System.Threading;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class RoomTests_Admin
    {
        private IWebDriver driver;
        private RoomPage roomPage;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            roomPage = new RoomPage(driver);

            // Tự động Login (Giống bài trước)
            driver.Navigate().GoToUrl("http://localhost:5000/login.html");
            driver.FindElement(By.Id("email")).SendKeys("tnct1@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("12345Tn");
            driver.FindElement(By.Name("login")).Click();
            Thread.Sleep(1000); // Đợi load

            // Xử lý swal OK nếu có (tùy logic web bạn)
            try { driver.FindElement(By.CssSelector(".swal2-confirm")).Click(); } catch { }

            // Chuyển sang trang Quản lý Resort/Phòng
            driver.Navigate().GoToUrl("http://localhost:5500/admin/rooms.html"); // Đổi URL theo đường dẫn thực tế của bạn
            Thread.Sleep(1000);
        }

        // ================= TẠO RESORT =================
        [Test]
        public void TC_HST_01_CreateResort_HappyPath()
        {
            roomPage.ClickCreateResort();
            roomPage.HandleResortPrompt("VungTau Homestay", true); // Nhập tên và bấm OK
            // Assert kiểm tra tạo thành công
        }

        [Test]
        public void TC_HST_02_CreateResort_EmptyName()
        {
            roomPage.ClickCreateResort();
            roomPage.HandleResortPrompt("", true); // Bỏ trống và bấm OK
            // Assert kiểm tra báo lỗi
        }

        [Test]
        public void TC_HST_03_CreateResort_Cancel()
        {
            roomPage.ClickCreateResort();
            roomPage.HandleResortPrompt("Test Hủy", false); // Bấm Cancel
        }

        // ================= TẠO PHÒNG MỚI =================
        [Test]
        public void TC_ROM_01_AddRoom_HappyPath()
        {
            roomPage.ClickAddRoom();
            // id Loại phòng: Gia đình | Cấu hình giường: 1 Giường Đơn | Location: Đà Lạt
            roomPage.EnterRoomData("2e6bde76-3d94-4073-880b-f3611b62c594", "1500000", "1 giường đơn", "available", "da-lat");
            roomPage.ClickSaveRoom();
        }

        [Test]
        public void TC_ROM_02_AddRoom_EmptyLocation()
        {
            roomPage.ClickAddRoom();
            roomPage.EnterRoomData("2e6bde76-3d94-4073-880b-f3611b62c594", "1500000", "1 giường đơn", "available", ""); // Bỏ trống Location
            roomPage.ClickSaveRoom();
        }

        [Test]
        public void TC_ROM_05_AddRoom_NegativePrice()
        {
            roomPage.ClickAddRoom();
            roomPage.EnterRoomData("2e6bde76-3d94-4073-880b-f3611b62c594", "-500000", "1 giường đơn", "available", "da-lat");
            roomPage.ClickSaveRoom(); // Nên có validation báo lỗi
        }

        [Test]
        public void TC_ROM_09_AddRoom_Cancel()
        {
            roomPage.ClickAddRoom();
            roomPage.EnterRoomData("2e6bde76-3d94-4073-880b-f3611b62c594", "1500000", "", "", "");
            roomPage.ClickCancelRoom(); // Đóng Modal
        }

        // ================= SỬA PHÒNG =================
        [Test]
        public void TC_ROM_10_EditRoom_UpdatePrice()
        {
            roomPage.ClickFirstEditRoom();
            driver.FindElement(By.Id("room-price")).Clear();
            driver.FindElement(By.Id("room-price")).SendKeys("300000"); // Sửa giá
            roomPage.ClickSaveRoom();
        }

        [Test]
        public void TC_ROM_12_EditRoom_ClearPrice()
        {
            roomPage.ClickFirstEditRoom();
            driver.FindElement(By.Id("room-price")).Clear(); // Cố tình xóa trắng
            roomPage.ClickSaveRoom();
        }

        [Test]
        public void TC_ROM_15_EditRoom_LongDescription()
        {
            roomPage.ClickFirstEditRoom();
            driver.FindElement(By.Id("room-description")).Clear();
            driver.FindElement(By.Id("room-description")).SendKeys("Tọa lạc ở Đà Lạt thuộc Lâm Đồng... Đây là một đoạn mô tả rất dài để test");
            roomPage.ClickSaveRoom();
        }

        // ================= XÓA PHÒNG =================
        [Test]
        public void TC_ROM_20_DeleteRoom_Accept()
        {
            roomPage.ClickFirstDeleteRoom();
            roomPage.HandleDeleteConfirm(true); // Bấm OK
        }

        [Test]
        public void TC_ROM_21_DeleteRoom_Cancel()
        {
            roomPage.ClickFirstDeleteRoom();
            roomPage.HandleDeleteConfirm(false); // Bấm Cancel
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}