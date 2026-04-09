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

namespace Nhom_214.Tests
{
    [TestFixture]
    public class RoomTests
    {
        private IWebDriver driver;
        private HomePage homePage;
        private static string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static string excelFilePath = Path.Combine(projectRoot, "TestData", "Report_Nhom_214.xlsx");
        private string screenshotFolder = Path.Combine(projectRoot, "TestResults", "Screenshots");

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            homePage = new HomePage(driver);
        }

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
                    testCases.Add(new TestCaseData(
                        row.Cell(1).GetString().Trim(),
                        row.Cell(2).GetString().Trim(),
                        row.Cell(3).GetString().Trim(),
                        row.Cell(4).GetString().Trim(),
                        rowIdx
                    ).SetName(row.Cell(1).GetString()));
                }
            }
            return testCases;
        }

        [Test, TestCaseSource(nameof(GetRoomUserData))]
        public void ExecuteRoomUserTest(string tcId, string action, string location, string expected, int rowIndex)
        {
            string actualMsg = ""; bool isPass = false;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            driver.Navigate().GoToUrl("http://localhost:5000/home.html");

            try
            {
                if (action.ToLower() == "search")
                {
                    homePage.Search(location);
                    var container = wait.Until(d => d.FindElement(By.Id("room-cards-container")));
                    string resultText = container.Text.ToLower();

                    if (expected.ToLower().Contains("không tìm thấy"))
                    {
                        isPass = resultText.Contains("không tìm thấy");
                        actualMsg = isPass ? "Đúng: Báo không tìm thấy" : "Lỗi: Vẫn hiện kết quả";
                    }
                    else
                    {
                        isPass = resultText.Contains(expected.ToLower());
                        actualMsg = isPass ? $"Đúng: Thấy {expected}" : "Lỗi: Không thấy phòng";
                    }
                }
                else if (action.ToLower() == "checkdetail")
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, 500);");
                    var firstRoom = wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("room-card")));
                    firstRoom.Click();
                    var img = wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("img")));
                    isPass = img.Displayed;
                    actualMsg = isPass ? "Load ảnh thành công" : "Không load được ảnh";
                }
            }
            catch (Exception ex) { actualMsg = "Lỗi: " + ex.Message; }

            WriteResultToExcel(rowIndex, actualMsg, isPass, tcId);
            Assert.Multiple(() =>
            {
                // Sử dụng Assert.That để NUnit tự điền vào Expected và But was
                Assert.That(actualMsg.ToLower(), Does.Contain(expected.ToLower()), $"TC_ID: {tcId} thất bại!");
            });
        }

        private void WriteResultToExcel(int rowIndex, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("RoomUser");
                ws.Cell(rowIndex, 6).Value = actual;      // 
                ws.Cell(rowIndex, 7).Value = isPass ? "PASS" : "FAIL"; // 

                if (!isPass)
                {
                    string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:HHmmss}.png");
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                    ws.Cell(rowIndex, 8).Value = "Xem ảnh lỗi"; // 
                    ws.Cell(rowIndex, 8).SetHyperlink(new XLHyperlink(path));
                }

                workbook.Save();
                // In ra Console để Thiên nhìn thấy ngay trong Test Explorer mà không cần mở Excel
                Console.WriteLine($"[RESULT] {tcId}: {actual} -> {(isPass ? "Passed" : "Failed")}");
            }
        }

        [TearDown]
        public void Teardown() { driver?.Quit();driver?.Dispose(); }
    }
}