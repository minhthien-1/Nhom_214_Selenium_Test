using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using System;
using System.Threading;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.CreateDriver();
            // Đảm bảo URL khớp với file HTML của bạn
            driver.Navigate().GoToUrl("http://localhost:5000/login.html");
            loginPage = new LoginPage(driver);
        }

        // --- NHÓM 1: ĐĂNG NHẬP THÀNH CÔNG ---
        [TestCase("letho@gmail.com", "12345Tn@", TestName = "Đăng_Nhập_01_Thành_Công_Khách_Hàng")]
        [TestCase("tnct1@gmail.com", "12345Tn", TestName = "Đăng_Nhập_02_Thành_Công_Quản_Trị_Viên")]
        public void Dang_Nhap_Thanh_Cong(string u, string p)
        {
            loginPage.Login(u, p);

            // Xử lý Popup SweetAlert "Đăng nhập thành công!"
            // Hàm này sẽ lấy text và tự động nhấn nút OK cho bạn
            string msg = loginPage.HandleSweetAlert();

            Assert.That(msg.ToLower(), Does.Contain("thành công"));

            // Sau khi tắt Popup, đợi trình duyệt chuyển hướng (về home.html hoặc admin)
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(d => d.Url.Contains("home") || d.Url.Contains("admin") || d.PageSource.Contains("Đăng xuất"));

            Assert.That(driver.PageSource, Does.Contain("Đăng xuất"));
        }

        // --- NHÓM 2: ĐĂNG NHẬP THẤT BẠI ---
        [TestCase("", "", "nhập đầy đủ", TestName = "Đăng_Nhập_03_Bỏ_Trống_Thông_Tin")]
        [TestCase("minhthien@gmail.com", "", "nhập đầy đủ", TestName = "Đăng_Nhập_04_Bỏ_Trống_Mật_Khẩu")]
        [TestCase("taikhoankhongton_tai@gmail.com", "123", "không tồn tại", TestName = "Đăng_Nhập_05_Tài_Khoản_Không_Tồn_Tại")]
        [TestCase("letho@gmail.com", "matkhausai123", "đăng nhập thất bại sai mật khẩu", TestName = "Đăng_Nhập_06_Sai_Mật_Khẩu")]
        public void Dang_Nhap_That_Bai(string u, string p, string expectedErr)
        {
            loginPage.Login(u, p);

            // Gọi hàm xử lý SweetAlert để lấy nội dung lỗi và nhấn OK để giải phóng màn hình
            string actualError = loginPage.HandleSweetAlert();

            // Kiểm tra xem thông báo lỗi có chứa từ khóa mong muốn không
            Assert.That(actualError.ToLower(), Does.Contain(expectedErr.ToLower()));
        }

        // --- NHÓM 3: ĐĂNG XUẤT ---
        [Test]
        public void Dang_Xuat_01_Kiem_Tra_Chuc_Nang()
        {
            // 1. Thực hiện đăng nhập thành công trước
            loginPage.Login("letho@gmail.com", "12345Tn@");
            loginPage.HandleSweetAlert(); // Nhấn OK trên Popup thành công

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Đợi cho đến khi nút Đăng xuất xuất hiện trên màn hình
            IWebElement logoutBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), 'Đăng xuất')]")));

            // 2. Bấm nút đăng xuất
            logoutBtn.Click();

            // 3. Kiểm tra xem đã quay về trang đăng nhập chưa
            wait.Until(d => d.Url.Contains("login") || d.PageSource.Contains("Đăng nhập"));
            Assert.That(driver.PageSource, Does.Contain("Đăng nhập"));
        }

        [TearDown]
        public void Teardown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}