using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using ClosedXML.Excel;
using System;
using System.IO;
using System.Collections.Generic;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class VoucherTests
    {
        private IWebDriver driver;
        private VoucherPage voucherPage;
        private static string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static string excelFilePath = Path.Combine(projectRoot, "TestData", "Report_Nhom_214.xlsx");
        private string screenshotFolder = Path.Combine(projectRoot, "TestResults", "Screenshots");

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            voucherPage = new VoucherPage(driver);

            driver.Navigate().GoToUrl("http://localhost:5000/login.html");
            driver.FindElement(By.Id("email")).SendKeys("tnct1@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("12345Tn");
            driver.FindElement(By.Name("login")).Click();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.CssSelector(".swal2-confirm"))).Click();
            wait.Until(d => d.FindElement(By.CssSelector("a[href='./voucher.html']"))).Click();
        }

        public static IEnumerable<TestCaseData> GetVoucherData()
        {
            var testCases = new List<TestCaseData>();
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet("Voucher");
                bool isFirst = true; int rowIdx = 0;
                foreach (var row in worksheet.RowsUsed())
                {
                    rowIdx++;
                    if (isFirst) { isFirst = false; continue; }
                    testCases.Add(new TestCaseData(
                        row.Cell(1).GetString().Trim(), row.Cell(2).GetString().Trim(),
                        row.Cell(3).GetString().Trim(), row.Cell(4).GetString().Trim(),
                        row.Cell(5).GetString().Trim(), row.Cell(6).GetString().Trim(),
                        row.Cell(7).GetString().Trim(), row.Cell(8).GetString().Trim(),
                        row.Cell(9).GetString().Trim(), rowIdx
                    ).SetName(row.Cell(1).GetString()));
                }
            }
            return testCases;
        }

        [Test, TestCaseSource(nameof(GetVoucherData))]
        public void ExecuteVoucherTest(string tcId, string action, string code, string name, string type, string val, string max, string expDate, string expected, int rowIndex)
        {
            string actual = ""; bool isPass = false;
            
            try
            {
                switch (action.ToLower())
                {
                    case "create":
                        voucherPage.ClickAddVoucher();
                        voucherPage.EnterVoucherData(code, name, type, val, max, expDate);
                        voucherPage.ClickSave();
                        actual = "Thành công"; break;
                    case "search":
                        voucherPage.SearchVoucher(code);
                        actual = voucherPage.GetSearchResultCount() > 0 ? "Tồn tại" : "Không tìm thấy";
                        break;
                }
                if (actual.ToLower().Contains(expected.ToLower())) isPass = true;
            }
            catch (Exception ex) { actual = "Lỗi: " + ex.Message; }

            WriteResultToExcel(rowIndex, actual, isPass, tcId);
            Assert.Multiple(() =>
            {
                // Sử dụng Assert.That để NUnit tự điền vào Expected và But was
                Assert.That(actual.ToLower(), Does.Contain(expected.ToLower()), $"TC_ID: {tcId} thất bại!");
            });
        }

        private void WriteResultToExcel(int rowIndex, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("Voucher");
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
        public void TearDown() { driver?.Quit(); driver?.Dispose(); }
    }
}