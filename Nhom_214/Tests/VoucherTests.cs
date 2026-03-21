using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using System;
using System.Threading;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class VoucherTests
    {
        private IWebDriver driver;
        private VoucherPage voucherPage;

        [SetUp]
        public void Setup()
        {
            // 1. Khởi tạo Browser từ DriverFactory
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            // Khởi tạo POM
            voucherPage = new VoucherPage(driver);

            // 2. Đi tới trang LOGIN cổng 5500
            driver.Navigate().GoToUrl("http://localhost:5000/login.html");

            // 3. Thực hiện ĐĂNG NHẬP
            driver.FindElement(By.Id("email")).SendKeys("tnct1@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("12345Tn");
            driver.FindElement(By.Name("login")).Click();

            // 4. Xử lý popup SweetAlert2
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            var swalBtn = wait.Until(d => d.FindElement(By.CssSelector(".swal2-confirm")));
            swalBtn.Click();

            // Đợi 1 giây cho trang Admin load hẳn
            Thread.Sleep(1000);

            // 5. Bấm vào menu Voucher
            driver.FindElement(By.CssSelector("a.nav__item[href='./voucher.html']")).Click();
        }

        // ==========================================
        // FEATURE 1: TẠO VOUCHER (5 TEST CASES)
        // ==========================================

        [Test]
        public void CreateVoucher_HappyPath_FixedAmount()
        {
            voucherPage.ClickAddVoucher();
            voucherPage.EnterVoucherData("RESORT2026", "Khuyến mãi mùa hè", "fixed", "500000", "100", "12/31/2026", "Giảm 500k");
            voucherPage.ClickSave();
            // Assert kiểm tra Toast message thành công...
        }

        [Test]
        public void CreateVoucher_HappyPath_Percentage()
        {
            voucherPage.ClickAddVoucher();
            voucherPage.EnterVoucherData("SALE10", "Giảm 10% dịp lễ", "percentage", "10", "50", "10/20/2026");
            voucherPage.ClickSave();
        }

        [Test]
        public void CreateVoucher_Validation_EmptyCode()
        {
            voucherPage.ClickAddVoucher();
            // Cố tình bỏ trống tham số Code (tham số đầu tiên)
            voucherPage.EnterVoucherData("", "Thiếu mã", "percentage", "10", "10", "12/31/2026");
            voucherPage.ClickSave();
            // Assert kiểm tra hệ thống chặn lưu và báo lỗi
        }

        [Test]
        public void CreateVoucher_LogicError_PercentGreaterThan100()
        {
            voucherPage.ClickAddVoucher();
            voucherPage.EnterVoucherData("ERROR101", "Test Lỗi Phần Trăm", "percentage", "105", "50", "10/10/2026");
            voucherPage.ClickSave();
            // Assert kiểm tra hệ thống báo lỗi > 100%
        }

        [Test]
        public void CreateVoucher_LogicError_PastExpiryDate()
        {
            voucherPage.ClickAddVoucher();
            voucherPage.EnterVoucherData("PASTDATE", "Voucher Hết Hạn", "fixed", "50000", "10", "01/01/2025");
            voucherPage.ClickSave();
        }

        // ==========================================
        // FEATURE 2: SỬA VOUCHER (5 TEST CASES)
        // ==========================================

        [Test]
        public void EditVoucher_HappyPath_UpdateName()
        {
            voucherPage.ClickFirstEditVoucher();
            driver.FindElement(By.Id("voucherName")).Clear();
            driver.FindElement(By.Id("voucherName")).SendKeys("Voucher Nghỉ Dưỡng VIP ✨");
            voucherPage.ClickSave();
        }

        [Test]
        public void EditVoucher_HappyPath_ChangeTypeAndValue()
        {
            voucherPage.ClickFirstEditVoucher();
            var typeSelect = new SelectElement(driver.FindElement(By.Id("discountType")));
            typeSelect.SelectByValue("fixed");
            driver.FindElement(By.Id("discountValue")).Clear();
            driver.FindElement(By.Id("discountValue")).SendKeys("200000");
            voucherPage.ClickSave();
        }

        [Test]
        public void EditVoucher_Validation_ClearCode()
        {
            voucherPage.ClickFirstEditVoucher();
            driver.FindElement(By.Id("voucherCode")).Clear(); // Xóa trắng mã
            voucherPage.ClickSave();
        }

        [Test]
        public void EditVoucher_LogicError_NegativeMaxUses()
        {
            voucherPage.ClickFirstEditVoucher();
            driver.FindElement(By.Id("maxUses")).Clear();
            driver.FindElement(By.Id("maxUses")).SendKeys("-50"); // Nhập số âm
            voucherPage.ClickSave();
        }

        [Test]
        public void EditVoucher_UIUX_CancelChanges()
        {
            voucherPage.ClickFirstEditVoucher();
            driver.FindElement(By.Id("voucherName")).SendKeys("Dữ liệu rác...");
            voucherPage.ClickCancel(); // Bấm Hủy thay vì Lưu
            // Assert kiểm tra Modal đóng và dữ liệu cũ không bị đổi
        }

        // ==========================================
        // FEATURE 3: TÌM KIẾM VOUCHER (3 TEST CASES)
        // ==========================================

        [Test]
        [Description("TKV01_admin: Tìm kiếm voucher theo ký tự chữ")]
        public void TKV01_admin_SearchByLetter_R()
        {
            voucherPage.SearchVoucher("R");
            Thread.Sleep(1000);

            int resultCount = voucherPage.GetSearchResultCount();
            Assert.IsTrue(resultCount > 0, "LỖI: Không hiển thị kết quả nào khi tìm chữ 'R'");
        }

        [Test]
        [Description("TKV02_admin: Tìm kiếm voucher theo ký tự số")]
        public void TKV02_admin_SearchByNumber_1()
        {
            voucherPage.SearchVoucher("1");
            Thread.Sleep(1000);

            int resultCount = voucherPage.GetSearchResultCount();
            Assert.IsTrue(resultCount > 0, "LỖI: Không hiển thị kết quả nào khi tìm số '1'");
        }

        [Test]
        [Description("TKV03_admin: Tìm kiếm chuỗi không tồn tại")]
        public void TKV03_admin_SearchNoResult_123()
        {
            voucherPage.SearchVoucher("123");
            Thread.Sleep(1000);

            int resultCount = voucherPage.GetSearchResultCount();
            Assert.AreEqual(0, resultCount, "LỖI: Tìm chuỗi không tồn tại nhưng vẫn ra kết quả!");
        }

        [TearDown]
        public void TearDown()
        {
            // Dọn dẹp đóng Browser sau mỗi test
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}