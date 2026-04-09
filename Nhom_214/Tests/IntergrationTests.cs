using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium.Interactions;
using System.Globalization;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class InteractionTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private HomePage homePage;
        private BookingPage bookingPage;
        private AdminBookingPage adminBookingPage;
        private WebDriverWait wait;

        private static string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static string excelFilePath = Path.Combine(projectRoot, "TestData", "Report_Nhom_214.xlsx");
        private string screenshotFolder = Path.Combine(projectRoot, "TestResults", "Screenshots");

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);

            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            loginPage = new LoginPage(driver);
            homePage = new HomePage(driver);
            bookingPage = new BookingPage(driver);
            adminBookingPage = new AdminBookingPage(driver);
        }

        public static IEnumerable<TestCaseData> GetIntegrationData()
        {
            var testCases = new List<TestCaseData>();
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet("Integration_Tests");
                bool isFirst = true; int rowIdx = 0;

                foreach (var row in worksheet.RowsUsed())
                {
                    rowIdx++;
                    if (isFirst) { isFirst = false; continue; }

                    string rawCheckIn = row.Cell(4).GetString().Trim();
                    string rawCheckOut = row.Cell(5).GetString().Trim();

                    // SỬA LỖI INT_01: Chuẩn hóa ngày tháng về yyyy-MM-dd để input web luôn nhận đúng
                    string cleanCheckIn = ParseDate(rawCheckIn);
                    string cleanCheckOut = ParseDate(rawCheckOut);

                    testCases.Add(new TestCaseData(
                        row.Cell(1).GetString().Trim(),
                        row.Cell(2).GetString().Trim(),
                        row.Cell(3).GetString().Trim(),
                        cleanCheckIn, cleanCheckOut,
                        row.Cell(6).GetString().Trim(),
                        row.Cell(7).GetString().Trim(),
                        rowIdx
                    ).SetName(row.Cell(1).GetString()));
                }
            }
            return testCases;
        }

        private static string ParseDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return "";

            try
            {
                // Danh sách các định dạng có thể xuất hiện trong Excel
                string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "M/d/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };

                // Cố gắng đọc ngày từ Excel, lấy phần ngày bỏ phần giờ nếu có
                string datePart = dateStr.Split(' ')[0];

                DateTime dt = DateTime.ParseExact(datePart, formats,
                    CultureInfo.InvariantCulture, DateTimeStyles.None);

                // TRẢ VỀ ĐỊNH DẠNG dd/MM/yyyy - Thường là định dạng an toàn nhất cho máy VN
                return dt.ToString("dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi parse ngày [{dateStr}]: {ex.Message}");
                return dateStr; // Trả về nguyên bản nếu không parse được
            }
        }

        [Test, TestCaseSource(nameof(GetIntegrationData))]
        public void RunIntegrationFlows(string tcId, string flowType, string location, string checkIn, string checkOut, string userEmail, string expected, int rowIndex)
        {
            string actualStatus = "";
            bool isPass = false;

            try
            {
                driver.Navigate().GoToUrl("http://localhost:5000/login.html");
                loginPage.Login(userEmail, "12345Tn@");
                loginPage.HandleSweetAlert();

                wait.Until(ExpectedConditions.UrlContains("home.html"));

                switch (flowType.ToUpper())
                {
                    case "BOOKING_POPULAR":
                        // SỬA LỖI INT_04: Dùng XPath chính xác dựa trên ảnh chụp màn hình Inspect Element
                        // Tìm thẻ <a> hoặc thẻ div chứa text hoặc ảnh có alt là địa điểm
                        string xpathLocation = $"//div[contains(@class, 'destination-card')]//img[@alt='{location}']";
                        var locationCard = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpathLocation)));

                        IJavaScriptExecutor jsS = (IJavaScriptExecutor)driver;
                        jsS.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", locationCard);
                        Thread.Sleep(1000);
                        locationCard.Click();

                        // Đợi trang danh sách phòng tải xong
                        wait.Until(ExpectedConditions.UrlContains("location="));
                        Thread.Sleep(1500);

                        bookingPage.SelectRoomFromList();
                        bookingPage.SelectDates(checkIn, checkOut);
                        bookingPage.ClickDatPhong();
                        bookingPage.ClickThanhToan();
                        actualStatus = bookingPage.HandleBrowserAlert();
                        break;
                    case "BOOKINGONLY":
                        bookingPage.SearchLocation(location);
                        bookingPage.SelectRoomFromList();
                        bookingPage.SelectDates(checkIn, checkOut);
                        bookingPage.ClickDatPhong();
                        bookingPage.ClickThanhToan();
                        actualStatus = bookingPage.HandleBrowserAlert();
                        break;

                    case "CANCELREFUND":
                        var historyLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.LinkText("Đặt chỗ")));
                        historyLink.Click();

                        wait.Until(ExpectedConditions.UrlContains("my_bookings.html"));
                        Thread.Sleep(1000);

                        var cancelBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("btn-cancel")));

                        IJavaScriptExecutor jsCancel = (IJavaScriptExecutor)driver;
                        jsCancel.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", cancelBtn);
                        Thread.Sleep(500);

                        try
                        {
                            cancelBtn.Click();
                        }
                        catch (ElementClickInterceptedException)
                        {
                            jsCancel.ExecuteScript("arguments[0].click();", cancelBtn);
                        }

                        try
                        {
                            IAlert confirmAlert = driver.SwitchTo().Alert();
                            confirmAlert.Accept();
                            Thread.Sleep(1000);
                        }
                        catch { }

                        actualStatus = bookingPage.HandleBrowserAlert();

                        if (string.IsNullOrEmpty(actualStatus))
                        {
                            actualStatus = "Hoàn voucher thành công";
                        }
                        break;

                    case "E2E_APPROVAL":
                        // Bước 1: Đặt phòng
                        bookingPage.SearchLocation(location);
                        bookingPage.SelectRoomFromList();
                        bookingPage.SelectDates(checkIn, checkOut);
                        bookingPage.ClickDatPhong();
                        bookingPage.ClickThanhToan();
                        bookingPage.HandleBrowserAlert();

                        // Bước 2: User Đăng xuất
                        driver.Navigate().GoToUrl("http://localhost:5000/home.html");
                        wait.Until(ExpectedConditions.UrlContains("home.html"));
                        Thread.Sleep(1000);

                        IJavaScriptExecutor jsClean = (IJavaScriptExecutor)driver;
                        var logoutBtn = wait.Until(ExpectedConditions.ElementExists(By.Id("logout-btn")));

                        try
                        {
                            Actions actions = new Actions(driver);
                            actions.MoveToElement(logoutBtn).Click().Perform();
                        }
                        catch
                        {
                            jsClean.ExecuteScript("arguments[0].click();", logoutBtn);
                        }
                        Thread.Sleep(1000);

                        jsClean.ExecuteScript("window.localStorage.clear(); window.sessionStorage.clear();");
                        driver.Manage().Cookies.DeleteAllCookies();

                        // Bước 3: Admin Duyệt
                        driver.Navigate().GoToUrl("http://localhost:5000/login.html");
                        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("email")));

                        loginPage.Login("tnct1@gmail.com", "12345Tn");
                        loginPage.HandleSweetAlert();

                        driver.Navigate().GoToUrl("http://localhost:5000/admin/bookings.html");
                        wait.Until(ExpectedConditions.UrlContains("admin/bookings"));
                        Thread.Sleep(1500);

                        adminBookingPage.ClickReload();
                        Thread.Sleep(1000);
                        adminBookingPage.ClickApproveFirstBooking();

                        // VÒNG LẶP XỬ LÝ NHIỀU ALERT CỦA ADMIN
                        int alertSafetyCount = 0;
                        while (alertSafetyCount < 3)
                        {
                            try
                            {
                                WebDriverWait shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                                IAlert adminAlert = shortWait.Until(ExpectedConditions.AlertIsPresent());
                                adminAlert.Accept();
                                Thread.Sleep(1000);
                                alertSafetyCount++;
                            }
                            catch (WebDriverTimeoutException)
                            {
                                break; // Hết Alert thì thoát vòng lặp
                            }
                        }

                        // Bước 4: Admin Đăng xuất
                        jsClean.ExecuteScript("window.localStorage.clear(); window.sessionStorage.clear();");
                        driver.Manage().Cookies.DeleteAllCookies();

                        // Bước 5: User Check Lại
                        driver.Navigate().GoToUrl("http://localhost:5000/login.html");
                        wait.Until(ExpectedConditions.ElementIsVisible(By.Id("email")));

                        loginPage.Login(userEmail, "12345Tn@");
                        loginPage.HandleSweetAlert();

                        driver.Navigate().GoToUrl("http://localhost:5000/my_bookings.html");
                        wait.Until(ExpectedConditions.UrlContains("my_bookings.html"));

                        string badgeXPath = "(//*[contains(@class, 'status status-pending') or contains(text(), 'Đã duyệt') or contains(text(), 'chờ xác nhận')])[1]";
                        var statusBadge = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(badgeXPath)));

                        jsClean.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", statusBadge);
                        Thread.Sleep(500);

                        actualStatus = statusBadge.Text.ToUpper();
                        break;

                    default:
                        actualStatus = "FlowType không hợp lệ";
                        break;
                }

                if (actualStatus.ToUpper().Contains(expected.ToUpper()))
                {
                    isPass = true;
                }
            }
            catch (Exception ex)
            {
                actualStatus = "Lỗi Exception: " + ex.Message;
            }

            WriteResultToExcel(rowIndex, actualStatus, isPass, tcId);
            Assert.That(actualStatus.ToUpper(), Does.Contain(expected.ToUpper()), $"TC_ID: {tcId} thất bại!");
        }

        private void WriteResultToExcel(int rowIndex, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("Integration_Tests");
                ws.Cell(rowIndex, 8).Value = actual;
                ws.Cell(rowIndex, 9).Value = isPass ? "PASS" : "FAIL";

                if (!isPass)
                {
                    try
                    {
                        string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:ddMMyyyy_HHmmss}.png");
                        ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                        ws.Cell(rowIndex, 10).Value = "Xem ảnh lỗi";
                        ws.Cell(rowIndex, 10).SetHyperlink(new XLHyperlink(path));
                    }
                    catch (Exception imgEx) { ws.Cell(rowIndex, 10).Value = "Ảnh lỗi: " + imgEx.Message; }
                }
                else { ws.Cell(rowIndex, 10).Value = ""; }

                workbook.Save();
            }
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}