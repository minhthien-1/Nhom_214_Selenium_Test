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
    public class RegisterTests
    {
        private IWebDriver driver;
        private RegisterPage registerPage;
        private static string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static string excelFilePath = Path.Combine(projectRoot, "TestData", "Report_Nhom_214.xlsx");
        private string screenshotFolder = Path.Combine(projectRoot, "TestResults", "Screenshots");

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            registerPage = new RegisterPage(driver);
        }

        public static IEnumerable<TestCaseData> GetRegData()
        {
            var testCases = new List<TestCaseData>();
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet("Register");
                bool isFirst = true; int rowIdx = 0;
                foreach (var row in worksheet.RowsUsed())
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

        [Test, TestCaseSource(nameof(GetRegData))]
        public void ExecuteRegisterTest(string tcId, string name, string user, string email, string phone, string pass, string confirm, string expected, int rowIndex)
        {
            driver.Navigate().GoToUrl("http://localhost:5500/register.html");
            string actualMsg = ""; bool isPass = false;

            try
            {
                if (user.ToLower() == "random") user = "u" + DateTime.Now.Ticks.ToString().Substring(10);
                if (email.ToLower() == "random") email = user + "@test.com";

                registerPage.FillForm(name, user, email, phone, pass, confirm);
                actualMsg = registerPage.GetAlertTextWithWait();

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
                var ws = workbook.Worksheet("Register");
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
        public void Teardown() { driver?.Quit(); driver?.Dispose(); }
    }
}