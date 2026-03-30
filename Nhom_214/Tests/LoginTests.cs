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
    public class LoginTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private static string excelFilePath = @"C:\C#\Nhom_214_Selenium_Test\Report_Nhom_214.xlsx";
        private string screenshotFolder = @"C:\Users\Admin\OneDrive - Ho Chi Minh City University of Foreign Languages and Information Technology - HUFLIT\Pictures\TestFailures";

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
            driver.Navigate().GoToUrl("http://localhost:5500/login.html");

            try
            {
                if (action.ToLower() == "login")
                {
                    loginPage.Login(user, pass);
                    actualMsg = loginPage.HandleSweetAlert();

                    if (actualMsg.ToLower().Contains("thành công"))
                    {
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                        wait.Until(d => d.Url.Contains("home") || d.Url.Contains("admin") || d.PageSource.Contains("Đăng xuất"));
                    }
                }
                else if (action.ToLower() == "logout")
                {
                    loginPage.Login(user, pass);
                    loginPage.HandleSweetAlert();
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    var logoutBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), 'Đăng xuất')]")));
                    logoutBtn.Click();
                    wait.Until(d => d.Url.Contains("login") || d.PageSource.Contains("Đăng nhập"));
                    actualMsg = "Đăng nhập"; // Lấy keyword trang chủ
                }

                if (actualMsg.ToLower().Contains(expected.ToLower())) isPass = true;
            }
            catch (Exception ex) { actualMsg = "Lỗi Exception: " + ex.Message; }

            WriteResult(rowIndex, "Login", actualMsg, isPass, testCaseId);
            Assert.IsTrue(isPass, $"Test {testCaseId} FAILED. Actual: {actualMsg}");
        }

        private void WriteResult(int row, string sheet, string actual, bool isPass, string tcId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var ws = workbook.Worksheet(sheet);
                ws.Cell(row, 6).Value = actual;
                ws.Cell(row, 7).Value = isPass ? "PASS" : "FAIL";
                if (!isPass)
                {
                    string path = Path.Combine(screenshotFolder, $"{tcId}_{DateTime.Now:HHmmss}.png");
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path);
                    ws.Cell(row, 8).Value = "Link Ảnh";
                    ws.Cell(row, 8).SetHyperlink(new XLHyperlink(path));
                }
                workbook.Save();
            }
        }

        [TearDown]
        public void Teardown() { driver?.Quit(); driver?.Dispose(); }
    }
}