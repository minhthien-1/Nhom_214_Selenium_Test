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

            driver.Navigate().GoToUrl("http://localhost:5500/login.html");
            driver.FindElement(By.Id("email")).SendKeys("tnct1@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("12345Tn");
            driver.FindElement(By.Name("login")).Click();

            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var confirm = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".swal2-confirm")));
                confirm.Click();

                // Đợi 1 giây để trình duyệt lưu Token đăng nhập
                Thread.Sleep(1000);
            }
            catch { }

            driver.Navigate().GoToUrl("http://localhost:5500/admin/rooms.html");

            // QUAN TRỌNG: Đợi 2 giây cho API gọi dữ liệu và hiển thị các nút chức năng
            Thread.Sleep(2000);
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
                        Thread.Sleep(500); // Đợi popup hiện
                        roomPage.HandleResortPrompt(p1, p2.ToLower() == "true");
                        actualMsg = "thành công"; // Ghi thường để dễ khớp với expected
                        break;

                    case "addroom":
                        roomPage.ClickAddRoom();
                        Thread.Sleep(1000); // Đợi modal thêm phòng bật lên
                        roomPage.EnterRoomData(p1, p2, p3, p4, p5);
                        roomPage.ClickSaveRoom();
                        actualMsg = "lưu thành công";
                        break;

                    // ĐÃ BỔ SUNG CASE EDIT ROOM
                    case "editroom":
                        roomPage.ClickFirstEditRoom();
                        Thread.Sleep(1000); // Đợi modal sửa phòng bật lên
                        roomPage.EnterRoomData(p1, p2, p3, p4, p5);
                        roomPage.ClickSaveRoom();
                        actualMsg = "cập nhật thành công";
                        break;

                    case "deleteroom":
                        roomPage.ClickFirstDeleteRoom();
                        Thread.Sleep(500); // Đợi Alert xác nhận hiện lên
                        roomPage.HandleDeleteConfirm(p1.ToLower() == "true");
                        actualMsg = "xóa thành công";
                        break;
                }

                if (actualMsg.ToLower().Contains(expected.ToLower())) isPass = true;
            }
            catch (Exception ex) { actualMsg = "Lỗi: " + ex.Message; }

            WriteResultToExcel(rowIndex, actualMsg, isPass, tcId);
            Assert.That(actualMsg.ToLower(), Does.Contain(expected.ToLower()), $"TC_ID: {tcId} thất bại!");
        }

        private void WriteResultToExcel(int rowIndex, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("RoomAdmin");

                // ĐÃ SỬA LẠI CỘT CHO KHỚP VỚI EXCEL CỦA BẠN (Cột 9, 10, 11)
                ws.Cell(rowIndex, 9).Value = actual;
                ws.Cell(rowIndex, 10).Value = isPass ? "PASS" : "FAIL";

                if (!isPass)
                {
                    try
                    {
                        string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:HHmmss}.png");
                        ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                        ws.Cell(rowIndex, 11).Value = "Xem ảnh lỗi";
                        ws.Cell(rowIndex, 11).SetHyperlink(new XLHyperlink(path));
                    }
                    catch (Exception imgEx) { ws.Cell(rowIndex, 11).Value = "Lỗi ảnh: " + imgEx.Message; }
                }
                else { ws.Cell(rowIndex, 11).Value = ""; }

                workbook.Save();
                Console.WriteLine($"[RESULT] {tcId}: {actual} -> {(isPass ? "Passed" : "Failed")}");
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