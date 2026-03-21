using Nhom_214.Pages;
using Nhom_214.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private WebDriverWait wait;

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.CreateDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            loginPage = new LoginPage(driver);
            profilePage = new ProfilePage(driver);
            bookingPage = new BookingPage(driver);
            adminPage = new AdminPage(driver);

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

        // --- NHÓM TÍCH HỢP: ĐỒNG BỘ DỮ LIỆU PROFILE GIỮA USER VÀ ADMIN ---
        //[Test]
        //[Description("INTER_KH_02: Thay đổi profile khách hàng và kiểm tra hiển thị phía Admin")]
        //public void INTER_KH_02_Profile_Sync_With_Admin()
        //{
        //    // 1. Khách hàng đổi tên trong Profile
        //    loginPage.Login("tnct@gmail.com", "12345Tn@");
        //    loginPage.HandleSweetAlert();
        //    driver.Navigate().GoToUrl("http://localhost:5000/profile.html");

        //    profilePage.UpdateFullName("nguyen van minh thien");
        //    loginPage.HandleSweetAlert();

        //    // 2. Logout và Login Admin
        //    driver.FindElement(By.Id("logoutBtn")).Click();
        //    loginPage.Login("admin@gmail.com", "12345Tn@");
        //    loginPage.HandleSweetAlert();

        //    // 3. Admin xem danh sách đơn hàng/khách hàng
        //    driver.Navigate().GoToUrl("http://localhost:5000/admin/customers.html");
        //    Assert.That(driver.PageSource, Does.Contain("nguyen van minh thien"));
        //}

        // --- NHÓM TÍCH HỢP: THAY ĐỔI DỮ LIỆU HỆ THỐNG (ẢNH PHÒNG) ---
        //[Test]
        //[Description("INTER_KH_03: Admin đổi ảnh phòng và kiểm tra hiển thị đơn hàng của khách")]
        //public void INTER_KH_03_Admin_Change_Image_Sync()
        //{
        //    // 1. Admin đăng nhập và đổi ảnh phòng
        //    loginPage.Login("admin@gmail.com", "12345T@");
        //    loginPage.HandleSweetAlert();
        //    adminPage.UpdateRoomImage("Room_ID_123", "dalat.jpg");

        //    // 2. Logout và Khách hàng đăng nhập lại
        //    driver.FindElement(By.Id("logoutBtn")).Click();
        //    loginPage.Login("tnct@gmail.com", "12345Tn@");
        //    loginPage.HandleSweetAlert();

        //    // 3. Kiểm tra ảnh trong trang đơn đặt phòng
        //    driver.Navigate().GoToUrl("http://localhost:5000/my-bookings.html");
        //    IWebElement roomImg = driver.FindElement(By.CssSelector(".booking-img"));
        //    Assert.That(roomImg.GetAttribute("src"), Does.Contain("dalat.jpg"));
        //}

        // --- NHÓM TÍCH HỢP: THAY ĐỔI EMAIL VÀ ĐĂNG NHẬP ---
        //[Test]
        //[Description("INTER_KH_04: Đổi email thành công và thử đăng nhập bằng email cũ")]
        //public void INTER_KH_04_Change_Email_Security_Check()
        //{
        //    // 1. Đổi email
        //    loginPage.Login("tnct@gmail.com", "12345Tn@");
        //    loginPage.HandleSweetAlert();
        //    driver.Navigate().GoToUrl("http://localhost:5000/profile.html");
        //    profilePage.UpdateEmail("thien@gmail.com");
        //    loginPage.HandleSweetAlert();

        //    // 2. Logout
        //    driver.FindElement(By.Id("logoutBtn")).Click();

        //    // 3. Thử đăng nhập lại bằng email cũ (tnct@gmail.com)
        //    loginPage.Login("tnct@gmail.com", "12345Tn@");
        //    string errorMsg = loginPage.HandleSweetAlert();

        //    Assert.That(errorMsg.ToLower(), Does.Contain("không tồn tại"));
        //}

        // --- NHÓM TÍCH HỢP: KIỂM TRA LỖI TRANG THANH TOÁN (BUG REPORT) ---
        //[Test]
        //[Description("INTER_KH_05: Kiểm tra lỗi không lấy được thông tin phòng khi thanh toán")]
        //public void INTER_KH_05_Payment_Info_Loss_Bug()
        //{
        //    loginPage.Login("tnct@gmail.com", "12345Tn@");
        //    loginPage.HandleSweetAlert();

        //    bookingPage.SearchLocation("Đà lạt");
        //    bookingPage.SelectDates("01/04/2026", "04/04/2026");
        //    bookingPage.ClickDatPhong();

        //    // Nhấn thanh toán
        //    bookingPage.ClickThanhToan();

        //    // Theo mô tả của bạn: Test case này sẽ FAIL do lỗi trang thanh toán không có thông tin
        //    bool isInfoDisplayed = bookingPage.IsPaymentDetailVisible();
        //    Assert.IsTrue(isInfoDisplayed, "LỖI: Trang thanh toán không hiển thị thông tin phòng!");
        //}

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
