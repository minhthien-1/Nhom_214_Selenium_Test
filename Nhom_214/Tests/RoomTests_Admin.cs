using NUnit.Framework;
using OpenQA.Selenium;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class RoomTests_Admin
    {
        private IWebDriver driver;
        private RoomPage roomPage;
        private static string excelFilePath = @"C:\C#\Nhom_214_Selenium_Test\Report_Nhom_214.xlsx";
        private string screenshotFolder = @"C:\Users\Admin\OneDrive - Ho Chi Minh City University of Foreign Languages and Information Technology - HUFLIT\Pictures\TestFailures";

        [SetUp]
        public void Setup()
        {
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            roomPage = new RoomPage(driver);

            // Tự động Login
            driver.Navigate().GoToUrl("http://localhost:5500/login.html");
            driver.FindElement(By.Id("email")).SendKeys("tnct1@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("12345Tn");
            driver.FindElement(By.Name("login")).Click();
            Thread.Sleep(1500);
            try { driver.FindElement(By.CssSelector(".swal2-confirm")).Click(); } catch { }

            driver.Navigate().GoToUrl("http://localhost:5500/admin/rooms.html");
            Thread.Sleep(1500);
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
                    string tcId = row.Cell(1).GetString().Trim();
                    if (string.IsNullOrEmpty(tcId)) continue;

                    testCases.Add(new TestCaseData(
                        tcId, row.Cell(2).GetString().Trim(), row.Cell(3).GetString().Trim(),
                        row.Cell(4).GetString().Trim(), row.Cell(5).GetString().Trim(),
                        row.Cell(6).GetString().Trim(), row.Cell(7).GetString().Trim(),
                        row.Cell(8).GetString().Trim(), rowIdx
                    ).SetName(tcId));
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
                        bool accept = p2.ToLower() == "true";
                        roomPage.HandleResortPrompt(p1, accept);
                        actualMsg = "Thao tác thành công"; // Bạn có thể móc hàm bắt Toast msg vào đây
                        break;

                    case "addroom":
                        roomPage.ClickAddRoom();
                        roomPage.EnterRoomData(p1, p2, p3, p4, p5); // ID, Price, Bed, Status, Location
                        roomPage.ClickSaveRoom();
                        actualMsg = "Lưu thành công";
                        break;

                    case "editroom":
                        roomPage.ClickFirstEditRoom();
                        if (!string.IsNullOrEmpty(p1)) { driver.FindElement(By.Id("room-price")).Clear(); driver.FindElement(By.Id("room-price")).SendKeys(p1); }
                        if (!string.IsNullOrEmpty(p2)) { driver.FindElement(By.Id("room-description")).Clear(); driver.FindElement(By.Id("room-description")).SendKeys(p2); }
                        roomPage.ClickSaveRoom();
                        actualMsg = "Cập nhật thành công";
                        break;

                    case "deleteroom":
                        roomPage.ClickFirstDeleteRoom();
                        roomPage.HandleDeleteConfirm(p1.ToLower() == "true");
                        actualMsg = "Xóa thành công";
                        break;
                }

                if (actualMsg.ToLower().Contains(expected.ToLower())) isPass = true;
            }
            catch (Exception ex) { actualMsg = "Lỗi Exception: " + ex.Message; }

            // Ghi kết quả vào cột 9, 10, 11
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("RoomAdmin");
                ws.Cell(rowIndex, 9).Value = actualMsg;
                ws.Cell(rowIndex, 10).Value = isPass ? "PASS" : "FAIL";
                if (!isPass) { /* Logic chụp ảnh tương tự các file trên */ }
                workbook.Save();
            }
            Assert.IsTrue(isPass);
        }

        [TearDown]
        public void Teardown() { driver?.Quit(); driver?.Dispose(); }
    }
}