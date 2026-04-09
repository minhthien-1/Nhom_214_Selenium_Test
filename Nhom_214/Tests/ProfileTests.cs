using ClosedXML.Excel;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class ProfileTests
    {
        private IWebDriver driver;
        private ProfilePage profilePage;
        private LoginPage loginPage;

        // Đường dẫn file (Thiên kiểm tra lại tên file .xlsx của mình nhé)
        private static string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static string excelPath = Path.Combine(projectRoot, "TestData", "Report_Nhom_214.xlsx");
        private string screenFolder = Path.Combine(projectRoot, "TestResults", "Screenshots");

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenFolder)) Directory.CreateDirectory(screenFolder);
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            profilePage = new ProfilePage(driver);
            loginPage = new LoginPage(driver);

            // 1. Vào trang Login và đăng nhập để lấy quyền vào Profile
            driver.Navigate().GoToUrl("http://localhost:5000/login.html");
            loginPage.Login("tnc1@gmail.com", "12345Tn@"); // Tài khoản mặc định để test
            loginPage.HandleSweetAlert();

            // 2. Chuyển hướng sang trang Profile
            driver.Navigate().GoToUrl("http://localhost:5000/profile.html");
        }

        [Test, TestCaseSource(nameof(GetProfileData))]
        // Tổng cộng có 8 tham số (NUnit hỗ trợ tối đa 9, nên 8 là con số an toàn)
        public void ExecuteProfileTest(string tcId, string action, string newVal, string fullName, string phone, string passData, string expected, int rowIdx)
        {
            string actual = "";
            bool isPass = false;

            try
            {
                switch (action.ToLower())
                {
                    case "edit_name":
                        // Test tên mới (newVal), phone không truyền vào nên Page sẽ "để yên"
                        profilePage.UpdateInfo(fullname: newVal);
                        break;

                    case "edit_email":
                        // Test email mới, truyền fullName để tránh lỗi web bắt buộc nhập
                        profilePage.UpdateInfo(email: newVal, fullname: fullName);
                        break;

                    case "edit_phone":
                        // ĐANG TEST PHONE: Xóa số cũ, điền số mới (newVal)
                        profilePage.UpdateInfo(phone: newVal, fullname: fullName);
                        break;

                    case "edit_username":
                        profilePage.UpdateInfo(username: newVal, fullname: fullName);
                        break;

                    case "change_pass":
                        // Tách cụm mật khẩu đã gom từ Excel
                        var parts = passData.Split('|');
                        profilePage.ChangePassword(parts[0], parts[1], parts[2]);
                        break;

                    case "delete_acc":
                        profilePage.DeleteAccount();
                        break;

                    case "back_button":
                        profilePage.ClickBack();
                        break;

                    default:
                        actual = "Action không hợp lệ";
                        break;
                }

                // Lấy thông báo nếu chưa có lỗi action
                if (string.IsNullOrEmpty(actual))
                {
                    System.Threading.Thread.Sleep(1000);
                    actual = profilePage.GetProfileNotification();
                }

                // So sánh kết quả
                if (actual.ToLower().Contains(expected.ToLower().Trim())) isPass = true;
            }
            catch (Exception ex)
            {
                actual = "Lỗi script: " + ex.Message;
            }

            // Ghi kết quả vào cột J, K, L
            WriteToExcel(rowIdx, actual, isPass, tcId);

            // Kiểm chứng kết quả để báo xanh/đỏ trong Test Explorer
            Assert.That(actual.ToLower(), Does.Contain(expected.ToLower().Trim()));
        }

        public static IEnumerable<TestCaseData> GetProfileData()
        {
            using (var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("ThongTinCN");
                var rows = ws.RowsUsed();
                bool isHeader = true;
                int rowIdx = 0;

                foreach (var row in rows)
                {
                    rowIdx++;
                    if (isHeader) { isHeader = false; continue; }

                    // Gom 3 cột mật khẩu (F, G, H) thành 1 chuỗi để giảm số lượng tham số
                    string oldP = row.Cell(6).GetString();
                    string newP = row.Cell(7).GetString();
                    string confP = row.Cell(8).GetString();
                    string passGroup = $"{oldP}|{newP}|{confP}";

                    yield return new TestCaseData(
                        row.Cell(1).GetString(), // A: tcId
                        row.Cell(2).GetString(), // B: action
                        row.Cell(3).GetString(), // C: newVal
                        row.Cell(4).GetString(), // D: fullName
                        row.Cell(5).GetString(), // E: phone
                        passGroup,               // F+G+H: gom nhóm mật khẩu
                        row.Cell(9).GetString(), // I: expected
                        rowIdx                   // Chỉ số dòng để ghi file
                    ).SetName(row.Cell(1).GetString());
                }
            }
        }

        private void WriteToExcel(int row, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("ThongTinCN");
                ws.Cell(row, 10).Value = actual; // Cột J
                ws.Cell(row, 11).Value = isPass ? "PASS" : "FAIL"; // Cột K

                if (!isPass)
                {
                    string fileName = $"{tcId}_{DateTime.Now:HHmmss}.png";
                    string path = Path.Combine(screenFolder, fileName);
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);

                    ws.Cell(row, 12).Value = "Xem ảnh lỗi"; // Cột L
                    ws.Cell(row, 12).SetHyperlink(new XLHyperlink(path));
                }
                workbook.Save();
            }
        }

        [TearDown]
        public void TearDown() { driver?.Quit(); driver?.Dispose(); }
    }
}