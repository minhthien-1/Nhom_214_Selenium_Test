using NUnit.Framework;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class RoomTests
    {
        private IWebDriver driver;
        private HomePage homePage;

        private static string excelFilePath = @"C:\C#\Nhom_214_Selenium_Test\Report_Nhom_214.xlsx";
        private string screenshotFolder = @"C:\Users\Admin\OneDrive - Ho Chi Minh City University of Foreign Languages and Information Technology - HUFLIT\Pictures\TestFailures";

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);

            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            homePage = new HomePage(driver);
        }

        // ==========================================
        // HÀM LẤY DATA TỪ EXCEL
        // ==========================================
        public static IEnumerable<TestCaseData> GetRoomUserData()
        {
            var testCases = new List<TestCaseData>();
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("RoomUser");
                bool isFirst = true; int rowIdx = 0;

                foreach (var row in ws.RowsUsed())
                {
                    rowIdx++;
                    if (isFirst) { isFirst = false; continue; }

                    string tcId = row.Cell(1).GetString().Trim();
                    if (string.IsNullOrEmpty(tcId)) continue;

                    testCases.Add(new TestCaseData(
                        tcId,
                        row.Cell(2).GetString().Trim(), // Action
                        row.Cell(3).GetString().Trim(), // Location
                        row.Cell(4).GetString().Trim(), // Expected
                        rowIdx                          // RowIndex
                    ).SetName(tcId));
                }
            }
            return testCases;
        }

        // ==========================================
        // HÀM THỰC THI CHÍNH
        // ==========================================
        [Test, TestCaseSource(nameof(GetRoomUserData))]
        public void ExecuteRoomUserTest(string tcId, string action, string location, string expected, int rowIndex)
        {
            string actualMsg = "";
            bool isPass = false;

            try
            {
                // Luôn mở trang Home trước khi test (Sử dụng port 5500)
                driver.Navigate().GoToUrl("http://localhost:5500/home.html");
                Thread.Sleep(1000);

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                if (action.ToLower() == "search")
                {
                    homePage.Search(location);
                    Thread.Sleep(1500);

                    var container = wait.Until(d => d.FindElement(By.Id("room-cards-container")));
                    string resultText = container.Text.ToLower();

                    // Logic so sánh thông minh
                    if (expected.ToLower().Contains("không tìm thấy"))
                    {
                        // Mong đợi rỗng (Negative)
                        if (resultText.Contains("không tìm thấy"))
                        {
                            isPass = true;
                            actualMsg = "Đúng: Báo không tìm thấy";
                        }
                        else actualMsg = "Lỗi: Đáng lẽ không có phòng nhưng web lại hiện kết quả";
                    }
                    else
                    {
                        // Mong đợi có phòng (Positive)
                        if (!resultText.Contains("không tìm thấy") && resultText.Contains(expected.ToLower()))
                        {
                            isPass = true;
                            actualMsg = $"Đúng: Hiển thị phòng tại {expected}";
                        }
                        else actualMsg = $"Lỗi: Không tìm thấy phòng tại {expected}";
                    }
                }
                else if (action.ToLower() == "checkdetail")
                {
                    // Cuộn màn hình để hiện các thẻ phòng
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollBy(0, 600);");
                    Thread.Sleep(1500);

                    // Bấm vào thẻ phòng đầu tiên
                    var firstRoom = wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("room-card")));
                    firstRoom.Click();
                    Thread.Sleep(2000);

                    // Kiểm tra trang chi tiết có load được ảnh không
                    var img = wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("img")));
                    if (img.Displayed)
                    {
                        isPass = true;
                        actualMsg = "Hiển thị ảnh chi tiết thành công";
                    }
                    else
                    {
                        actualMsg = "Lỗi: Không load được ảnh phòng";
                    }
                }
                else
                {
                    actualMsg = "Action không hợp lệ trong file Excel";
                }
            }
            catch (Exception ex)
            {
                actualMsg = "Exception: " + ex.Message;
            }

            // GHI KẾT QUẢ XUỐNG EXCEL
            WriteResult(rowIndex, "RoomUser", actualMsg, isPass, tcId);
            Assert.IsTrue(isPass, $"Test {tcId} FAILED. Actual: {actualMsg}");
        }

        // ==========================================
        // HÀM GHI EXCEL VÀ CHỤP ẢNH
        // ==========================================
        private void WriteResult(int row, string sheet, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet(sheet);
                ws.Cell(row, 5).Value = actual; // Cột 5 là Actual
                ws.Cell(row, 6).Value = isPass ? "PASS" : "FAIL"; // Cột 6 là Result

                if (!isPass)
                {
                    string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:HHmmss}.png");
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                    ws.Cell(row, 7).Value = "Link Ảnh";
                    ws.Cell(row, 7).SetHyperlink(new XLHyperlink(path));
                }
                workbook.Save();
            }
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