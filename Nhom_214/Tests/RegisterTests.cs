using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using Nhom_214.Pages;
using Nhom_214.Utilities;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class RegisterTests
    {
        private IWebDriver driver;
        private RegisterPage registerPage;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.CreateDriver();
            // Đảm bảo đường dẫn này khớp với project web của bạn
            driver.Navigate().GoToUrl("http://localhost:5000/register.html");
            registerPage = new RegisterPage(driver);
        }

        // --- NHÓM KIỂM TRA LỖI NHẬP LIỆU (VALIDATION) ---

        [TestCase("", "thien123", "t@g.com", "0123456789", "12345", "12345", "Họ và tên không được để trống!", TestName = "Đăng_Ký_02_Bỏ_Trống_Họ_Tên")]
        [TestCase("Minh Thien", "", "t@g.com", "0123456789", "12345", "12345", "Tên đăng nhập không được để trống!", TestName = "Đăng_Ký_03_Bỏ_Trống_Username")]
        [TestCase("Minh Thien", "user_sieu_cap_dai_hon_20_ky_tu", "t@g.com", "0123456789", "12345", "12345", "không được vượt quá 20 ký tự!", TestName = "Đăng_Ký_04_Username_Quá_Dài")]
        [TestCase("Minh Thien", "user!@#", "t@g.com", "0123456789", "12345", "12345", "không được có ký tự đặc biệt!", TestName = "Đăng_Ký_05_Username_Có_Ký_Tự_Đặc_Biệt")]
        [TestCase("Minh Thien", "thien123", "", "0123456789", "12345", "12345", "Email không được để trống!", TestName = "Đăng_Ký_06_Bỏ_Trống_Email")]
        [TestCase("Minh Thien", "thien123", "email_khong_hop_le", "0123456789", "12345", "12345", "Email không hợp lệ!", TestName = "Đăng_Ký_07_Email_Sai_Định_Dạng")]
        [TestCase("Minh Thien", "thien123", "t@g.com", "abc1234567", "12345", "12345", "chỉ được chứa số!", TestName = "Đăng_Ký_08_SĐT_Có_Chứa_Chữ")]
        [TestCase("Minh Thien", "thien123", "t@g.com", "1234567890", "12345", "12345", "phải bắt đầu từ số 0", TestName = "Đăng_Ký_09_SĐT_Không_Bắt_Đầu_Bằng_Số_0")]
        [TestCase("Minh Thien", "thien123", "t@g.com", "012345", "12345", "12345", "phải có 10 chữ số!", TestName = "Đăng_Ký_10_SĐT_Thiếu_Số")]
        [TestCase("Minh Thien", "thien123", "t@g.com", "0123456789", "123", "123", "ít nhất 5 ký tự!", TestName = "Đăng_Ký_11_Mật_Khẩu_Quá_Ngắn")]
        [TestCase("Minh Thien", "thien123", "t@g.com", "0123456789", "12345", "54321", "Mật khẩu nhập lại không khớp!", TestName = "Đăng_Ký_12_Xác_Nhận_Mật_Khẩu_Sai")]

        // --- NHÓM KIỂM TRA LỖI TỪ SERVER (Giả định data đã có trong DB) ---
        [TestCase("Admin", "admin", "admin@gmail.com", "0987654321", "12345", "12345", "Tên đăng nhập đã tồn tại!", TestName = "Đăng_Ký_13_Trùng_Username_Server")]
        [TestCase("Thien", "thiennew", "thien@gmail.com", "0123456789", "12345", "12345", "Email đã tồn tại!", TestName = "Đăng_Ký_14_Trùng_Email_Server")]

        public void Register_Validation_Tests(string n, string u, string e, string p, string pw, string cp, string expectedMsg)
        {
            registerPage.FillForm(n, u, e, p, pw, cp);
            string actualAlertText = registerPage.GetAlertTextWithWait();

            Assert.That(actualAlertText, Does.Contain(expectedMsg));
        }

        // --- TRƯỜNG HỢP THÀNH CÔNG ---
        [Test]
        public void Đăng_Ký_01_Thành_Công()
        {
            // Tạo username ngẫu nhiên để không bị lỗi trùng lặp khi chạy test nhiều lần
            string uniqueUser = "test" + System.DateTime.Now.Ticks.ToString().Substring(12);
            registerPage.FillForm("Người Dùng Thử", uniqueUser, uniqueUser + "@gmail.com", "0912345678", "123456", "123456");

            string actualAlertText = registerPage.GetAlertTextWithWait();
            Assert.That(actualAlertText, Does.Contain("Đăng ký thành công!"));

            Thread.Sleep(2000); // Chờ chuyển trang
            Assert.That(driver.Url, Does.Contain("login"));
        }

        [TearDown]
        public void Teardown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
                driver = null;
            }
        }
    }
}
