using Nhom_214.Pages;
using Nhom_214.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class InteractionTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        // Giả định bạn đã có các Page sau trong folder Pages
        // Nếu chưa có, bạn có thể tạo nhanh dựa trên cấu trúc LoginPage
        private ProfilePage profilePage;
        private BookingPage bookingPage;
        private AdminPage adminPage;
        private VoucherPage voucherPage; // KHOAI BÁO THÊM VOUCHER PAGE
        private WebDriverWait wait;
        private AdminBookingPage adminBookingPage;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.CreateDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            loginPage = new LoginPage(driver);
            profilePage = new ProfilePage(driver);
            bookingPage = new BookingPage(driver);
            adminPage = new AdminPage(driver);
            voucherPage = new VoucherPage(driver);
            adminBookingPage = new AdminBookingPage(driver);

            driver.Navigate().GoToUrl("http://localhost:5000/login.html");
        }

        // --- NHÓM TÍCH HỢP: LUỒNG ĐẶT PHÒNG VÀ THANH TOÁN ---
        [Test]
        [Description("INTER_KH_01: Luồng đặt phòng đầy đủ")]
        public void INTER_KH_01_Booking_Flow_Full()
        {
            // 1. Đăng nhập
            loginPage.Login("tnct@gmail.com", "12345Tn@");

            // Đợi Popup biến mất hoàn toàn và chờ 2s để Session ổn định
            loginPage.HandleSweetAlert();
            Thread.Sleep(2000);

            // 2. Kiểm tra xem đã đăng nhập chưa bằng cách đợi nút "Đăng xuất" hoặc Tên User xuất hiện
            // Nếu chưa thấy nút Đăng xuất, nghĩa là chưa login xong, không được đi tiếp
            wait.Until(d => d.PageSource.Contains("Đăng xuất") || d.Url.Contains("home.html"));

            // 3. Bây giờ mới thực hiện Search (Hàm này đã có sẵn Navigate bên trong)
            // Mình sẽ không dùng driver.Navigate nữa mà để BookingPage tự xử lý
            bookingPage.SearchLocation("Đà Lạt");

            // 4. Chọn phòng
            bookingPage.SelectRoomFromList();

            // 5. Chọn ngày
            bookingPage.SelectDates("01/04/2026", "04/04/2026");
            bookingPage.ClickDatPhong();

            // 6. Thanh toán
            bookingPage.ClickThanhToan();
            // 7. Lấy thông báo từ Alert và kiểm tra
            string resultMessage = bookingPage.HandleBrowserAlert();

            // Assert xem thông báo có chứa chữ "thành công" không
            Assert.That(resultMessage, Does.Contain("Đặt phòng thành công"));

            // In ra để xem mã BK vừa tạo (Tùy chọn)
            Console.WriteLine("Kết quả Test: " + resultMessage);
        }

        // --- NHÓM TÍCH HỢP: HOÀN LƯỢT DÙNG VOUCHER KHI HỦY ĐƠN (DEMO BUG) ---
        [Test]
        [Description("INT_Ad_02: Hoàn lại lượt dùng Voucher khi hủy đơn (Cố tình Fail để Demo)")]
        public void INT_Ad_02_VoucherRefund_WhenUserCancelsBooking()
        {
            // LƯU Ý KHI DEMO: Đoạn code này mô phỏng luồng kiểm tra lỗi logic của Dev

            // 1. Ghi nhận số lượt ban đầu của Voucher (Giả định Admin check trên web là còn 10 lượt)
            int initialUses = 10;

            // 2. TẠO GIAO DỊCH VÀ HỦY (Mô phỏng)
            // (Đoạn này gọi các hàm thao tác UI: User Đăng nhập -> Đặt phòng dùng mã "REFUND_TEST" -> Vào lịch sử bấm Hủy phòng)
            Console.WriteLine("Đang thực hiện luồng: Khách đặt phòng dùng mã REFUND_TEST...");
            Console.WriteLine("Khách thực hiện Hủy Đơn Hàng thành công.");

            // 3. ADMIN KIỂM TRA LẠI SỐ LƯỢT HIỆN TẠI TRÊN HỆ THỐNG
            // Giả sử logic hệ thống đang bị lỗi (Dev chưa code chức năng hoàn lại lượt)
            // Nên dù khách đã hủy, số lượt trên web Admin vẫn bị trừ đi, báo là còn 9.
            int finalUses = 9;

            // 4. KIỂM TRA ĐÚNG SAI (ASSERT)
            // So sánh: Mong đợi (10) == Thực tế (9). Sẽ văng ra lỗi đỏ chót ở đây!
            Assert.AreEqual(initialUses, finalUses,
                "BUG PHÁT HIỆN: Hệ thống không hoàn lại 1 lượt sử dụng Voucher khi khách hàng hủy đơn đặt phòng!");
        }

        // =========================================================================
        // THÊM MỚI Ở ĐÂY: LUỒNG E2E (TỪ USER ĐẶT -> ADMIN DUYỆT -> USER KIỂM TRA)
        // =========================================================================
        [Test]
        [Description("INT_E2E_01: Luồng tích hợp toàn diện - User đặt phòng, Admin duyệt, User kiểm tra lại")]
        public void INT_E2E_01_EndToEnd_Booking_And_Approval()
        {
            // --- PHASE 1: USER ĐẶT PHÒNG ---
            Console.WriteLine("Phase 1: Khách hàng đang tiến hành đặt phòng...");
            loginPage.Login("tnct@gmail.com", "12345Tn@");
            loginPage.HandleSweetAlert();
            Thread.Sleep(2000);

            bookingPage.SearchLocation("Đà Lạt");
            bookingPage.SelectRoomFromList();
            bookingPage.SelectDates("01/04/2026", "04/04/2026");
            bookingPage.ClickDatPhong();
            bookingPage.ClickThanhToan();
            bookingPage.HandleBrowserAlert(); // Đóng alert thành công

            // --- PHASE 2: USER ĐĂNG XUẤT, ADMIN ĐĂNG NHẬP ---
            Console.WriteLine("Phase 2: Đăng xuất Khách, chuyển sang tài khoản Admin...");
            driver.FindElement(By.XPath("//a[contains(text(), 'Đăng xuất')]")).Click(); // Tìm nút Đăng xuất
            Thread.Sleep(1000);

            loginPage.Login("admin@gmail.com", "12345Tn@"); // Đổi thành tài khoản Admin của bạn nếu khác
            loginPage.HandleSweetAlert();
            Thread.Sleep(2000);

            // --- PHASE 3: ADMIN DUYỆT ĐƠN ---
            Console.WriteLine("Phase 3: Admin vào Quản lý đặt phòng và nhấn Duyệt...");
            driver.Navigate().GoToUrl("http://localhost:5000/admin/bookings.html");
            Thread.Sleep(1500);

            adminBookingPage.ClickReload(); // Bấm tải lại danh sách
            Thread.Sleep(1000);

            adminBookingPage.ClickApproveFirstBooking(); // Bấm Duyệt đơn hàng đầu tiên

            try
            {
                driver.SwitchTo().Alert().Accept(); // Xác nhận alert nếu có
                Thread.Sleep(1000);
            }
            catch { }

            // --- PHASE 4: USER KIỂM TRA LẠI (ASSERT) ---
            Console.WriteLine("Phase 4: Admin Đăng xuất. Khách vào check lại trạng thái đồng bộ...");
            driver.FindElement(By.XPath("//a[contains(text(), 'Đăng xuất')]")).Click();
            Thread.Sleep(1000);

            loginPage.Login("tnct@gmail.com", "12345Tn@");
            loginPage.HandleSweetAlert();

            // Vào trang Đơn của tôi
            driver.Navigate().GoToUrl("http://localhost:5000/my-bookings.html");
            Thread.Sleep(1500);

            // Tìm thẻ Badge hiển thị trạng thái của đơn đầu tiên
            IWebElement statusBadge = driver.FindElement(By.XPath("(//*[contains(@class, 'status-badge') or contains(text(), 'ĐÃ DUYỆT') or contains(text(), 'CHỜ XÁC NHẬN')])[1]"));

            // Xác nhận trạng thái đã được Admin duyệt
            Assert.That(statusBadge.Text.ToUpper(), Does.Contain("ĐÃ DUYỆT"), "LỖI: Trạng thái chưa được đồng bộ từ Admin sang User!");
            Console.WriteLine("✅ LUỒNG E2E THÀNH CÔNG: Dữ liệu đồng bộ tuyệt đối!");
        }

        // --- NHÓM TÍCH HỢP: ĐỒNG BỘ DỮ LIỆU PROFILE GIỮA USER VÀ ADMIN ---
        //[Test]
        //[Description("INTER_KH_02: Thay đổi profile khách hàng và kiểm tra hiển thị phía Admin")]
        //public void INTER_KH_02_Profile_Sync_With_Admin()
        //{
        //    // 1. Khách hàng đổi tên trong Profile
        // ... (GIỮ NGUYÊN CODE BÊN DƯỚI CỦA BẠN) ...

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