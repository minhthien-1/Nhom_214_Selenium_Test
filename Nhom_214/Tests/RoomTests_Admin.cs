using NUnit.Framework;
using OpenQA.Selenium;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Collections.Generic;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class RoomTests_Admin
    {
        private IWebDriver driver;
        private RoomPage roomPage;
        private static string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static string excelFilePath = Path.Combine(projectRoot, "TestData", "Report_Nhom_214.xlsx");
        private string screenshotFolder = Path.Combine(projectRoot, "TestResults", "Screenshots");

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            roomPage = new RoomPage(driver);

            driver.Navigate().GoToUrl("http://localhost:5000/login.html");
            driver.FindElement(By.Id("email")).SendKeys("tnct1@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("12345Tn");
            driver.FindElement(By.Name("login")).Click();

            try
            {
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var confirm = wait.Until(d => d.FindElement(By.CssSelector(".swal2-confirm")));
                confirm.Click();
            }
            catch { }

            driver.Navigate().GoToUrl("http://localhost:5000/admin/rooms.html");
        }

        public static IEnumerable<TestCaseData> GetRoomData()
        {
            var testCases = new List<TestCaseData>();
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("RoomAdmin");
                bool isFirst = true; int rowIdx = 0;
                foreach (var row in ws.RowsUsed())
                {
                    rowIdx++;
                    if (isFirst) { isFirst = false; continue; }
                    testCases.Add(new TestCaseData(
                        row.Cell(1).GetString().Trim(), row.Cell(2).GetString().Trim(),
                        row.Cell(3).GetString().Trim(), row.Cell(4).GetString().Trim(),
                        row.Cell(5).GetString().Trim(), row.Cell(6).GetString().Trim(),
                        row.Cell(7).GetString().Trim(), row.Cell(8).GetString().Trim(), rowIdx
                    ).SetName(row.Cell(1).GetString()));
                }
            }
            return testCases;
        }

        [Test, TestCaseSource(nameof(GetRoomData))]
        public void ExecuteRoomAdminTest(string tcId, string action, string p1, string p2, string p3, string p4, string p5, string expected, int rowIndex)
        {
            string actualMsg = ""; bool isPass = false;
            try
            {
                switch (action.ToLower())
                {
                    case "createresort":
                        roomPage.ClickCreateResort();
                        roomPage.HandleResortPrompt(p1, p2.ToLower() == "true");
                        actualMsg = "Thành công"; break;
                    case "addroom":
                        roomPage.ClickAddRoom();
                        roomPage.EnterRoomData(p1, p2, p3, p4, p5);
                        roomPage.ClickSaveRoom();
                        actualMsg = "Lưu thành công"; break;
                    case "deleteroom":
                        roomPage.ClickFirstDeleteRoom();
                        roomPage.HandleDeleteConfirm(p1.ToLower() == "true");
                        actualMsg = "Xóa thành công"; break;
                }
                if (actualMsg.ToLower().Contains(expected.ToLower())) isPass = true;
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
                var ws = workbook.Worksheet("RoomAdmin");
                ws.Cell(rowIndex, 10).Value = actual;      // Cột J
                ws.Cell(rowIndex, 11).Value = isPass ? "PASS" : "FAIL"; // Cột K

                if (!isPass)
                {
                    string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:HHmmss}.png");
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                    ws.Cell(rowIndex, 12).Value = "Xem ảnh lỗi"; // Cột L
                    ws.Cell(rowIndex, 12).SetHyperlink(new XLHyperlink(path));
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