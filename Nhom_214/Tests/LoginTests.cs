using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private static string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        private static string excelFilePath = Path.Combine(projectRoot, "TestData", "Report_Nhom_214.xlsx");
        private string screenshotFolder = Path.Combine(projectRoot, "TestResults", "Screenshots");

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);
            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            loginPage = new LoginPage(driver);
        }

        public static IEnumerable<TestCaseData> GetLoginData()
        {
            var testCases = new List<TestCaseData>();
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet("Login");
                bool isFirst = true; int rowIdx = 0;
                foreach (var row in worksheet.RowsUsed())
                {
                    rowIdx++;
                    if (isFirst) { isFirst = false; continue; }
                    string tcId = row.Cell(1).GetString().Trim();
                    if (string.IsNullOrEmpty(tcId)) continue;

                    testCases.Add(new TestCaseData(
                        tcId, row.Cell(2).GetString().Trim(), row.Cell(3).GetString().Trim(),
                        row.Cell(4).GetString().Trim(), row.Cell(5).GetString().Trim(), rowIdx
                    ).SetName(tcId));
                }
            }
            return testCases;
        }

        [Test, TestCaseSource(nameof(GetLoginData))]
        public void ExecuteLoginTest(string testCaseId, string action, string user, string pass, string expected, int rowIndex)
        {
            string actualMsg = "";
            bool isPass = false;
            driver.Navigate().GoToUrl("http://localhost:5000/login.html");
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            try
            {
                if (action.ToLower() == "login")
                {
                    loginPage.Login(user, pass);
                    actualMsg = loginPage.HandleSweetAlert();

                    if (actualMsg.ToLower().Contains("thành công"))
                    {
                        wait.Until(d => d.Url.Contains("home") || d.Url.Contains("admin") || d.PageSource.Contains("Đăng xuất"));
                    }
                }
                else if (action.ToLower() == "logout")
                {
                    loginPage.Login(user, pass);
                    loginPage.HandleSweetAlert();
                    var logoutBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), 'Đăng xuất')]")));
                    logoutBtn.Click();
                    wait.Until(d => d.Url.Contains("login") || d.PageSource.Contains("Đăng nhập"));
                    actualMsg = "Đăng nhập";
                }

                if (actualMsg.ToLower().Contains(expected.ToLower())) isPass = true;
            }
            catch (Exception ex) { actualMsg = "Lỗi: " + ex.Message; }

            WriteResultToExcel(rowIndex, actualMsg, isPass, testCaseId);
            Assert.Multiple(() =>
            {
                // Sử dụng Assert.That để NUnit tự điền vào Expected và But was
                Assert.That(actualMsg.ToLower(), Does.Contain(expected.ToLower()), $"TC_ID: {testCaseId} thất bại!");
            });
        }

        private void WriteResultToExcel(int rowIndex, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet("Login");
                ws.Cell(rowIndex, 6).Value = actual;      // Cột J
                ws.Cell(rowIndex, 7).Value = isPass ? "PASS" : "FAIL"; // Cột K

                if (!isPass)
                {
                    string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:HHmmss}.png");
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                    ws.Cell(rowIndex, 8).Value = "Xem ảnh lỗi"; // Cột L
                    ws.Cell(rowIndex, 8).SetHyperlink(new XLHyperlink(path));
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