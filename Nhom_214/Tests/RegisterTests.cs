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
    public class RegisterTests
    {
        private IWebDriver driver;
        private RegisterPage registerPage;
        private static string excelFilePath = @"C:\C#\Nhom_214_Selenium_Test\Report_Nhom_214.xlsx";
        private string screenshotFolder = @"C:\Users\Admin\OneDrive - Ho Chi Minh City University of Foreign Languages and Information Technology - HUFLIT\Pictures\TestFailures";

        [SetUp]
        public void Setup()
        {
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
                // Tự động generate nếu yêu cầu test case thành công
                if (user.ToLower() == "random") user = "test" + DateTime.Now.Ticks.ToString().Substring(12);
                if (email.ToLower() == "random") email = user + "@gmail.com";

                registerPage.FillForm(name, user, email, phone, pass, confirm);
                actualMsg = registerPage.GetAlertTextWithWait();

                if (actualMsg.ToLower().Contains(expected.ToLower())) isPass = true;
            }
            catch (Exception ex) { actualMsg = "Lỗi Exception: " + ex.Message; }

            WriteResult(rowIndex, "Register", actualMsg, isPass, tcId);
            Assert.IsTrue(isPass);
        }

        private void WriteResult(int row, string sheet, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet(sheet);
                ws.Cell(row, 9).Value = actual;
                ws.Cell(row, 10).Value = isPass ? "PASS" : "FAIL";
                if (!isPass)
                {
                    string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:HHmmss}.png");
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                    ws.Cell(row, 11).Value = "Link Ảnh";
                    ws.Cell(row, 11).SetHyperlink(new XLHyperlink(path));
                }
                workbook.Save();
            }
        }

        [TearDown]
        public void Teardown() { driver?.Quit(); driver?.Dispose(); }
    }
}