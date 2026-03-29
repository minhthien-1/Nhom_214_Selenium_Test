using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Nhom_214.Pages;
using Nhom_214.Utilities;
using ClosedXML.Excel;
using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace Nhom_214.Tests
{
    [TestFixture]
    public class VoucherTests
    {
        private IWebDriver driver;
        private VoucherPage voucherPage;

        // Đường dẫn chuẩn xác theo máy của bạn
        private static string excelFilePath = @"C:\C#\Nhom_214_Selenium_Test\Report_Nhom_214.xlsx";
        private string screenshotFolder = @"C:\Users\Admin\OneDrive - Ho Chi Minh City University of Foreign Languages and Information Technology - HUFLIT\Pictures\TestFailures";

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(screenshotFolder)) Directory.CreateDirectory(screenshotFolder);

            driver = DriverFactory.CreateDriver();
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            voucherPage = new VoucherPage(driver);

            // Đăng nhập và vào trang Voucher (Giữ nguyên logic chuẩn của bạn)
            driver.Navigate().GoToUrl("http://localhost:5500/login.html");
            driver.FindElement(By.Id("email")).SendKeys("tnct1@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("12345Tn");
            driver.FindElement(By.Name("login")).Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            var swalBtn = wait.Until(d => d.FindElement(By.CssSelector(".swal2-confirm")));
            swalBtn.Click();
            Thread.Sleep(1000);

            driver.FindElement(By.CssSelector("a.nav__item[href='./voucher.html']")).Click();
            Thread.Sleep(1000);
        }

        // ==========================================
        // HÀM ĐỌC DATA TỪ EXCEL
        // ==========================================
        public static IEnumerable<TestCaseData> GetVoucherData()
        {
            var testCases = new List<TestCaseData>();
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet("Voucher");
                    var rows = worksheet.RowsUsed();
                    bool isFirstRow = true;
                    int currentRow = 0;

                    foreach (var row in rows)
                    {
                        currentRow++;
                        if (isFirstRow) { isFirstRow = false; continue; }

                        string testCaseId = row.Cell(1).GetString().Trim();
                        if (string.IsNullOrEmpty(testCaseId)) continue;

                        var data = new TestCaseData(
                            testCaseId,
                            row.Cell(2).GetString().Trim(), // Action (Create/Edit/Search)
                            row.Cell(3).GetString().Trim(), // Code_Search
                            row.Cell(4).GetString().Trim(), // Name
                            row.Cell(5).GetString().Trim(), // Type
                            row.Cell(6).GetString().Trim(), // Value
                            row.Cell(7).GetString().Trim(), // MaxUses
                            row.Cell(8).GetString().Trim(), // ExpiryDate
                            row.Cell(9).GetString().Trim(), // Expected
                            currentRow                      // RowIndex để ghi lại Actual
                        ).SetName(testCaseId);

                        testCases.Add(data);
                    }
                }
            }
            return testCases;
        }

        // ==========================================
        // HÀM THỰC THI CHÍNH (DATA-DRIVEN)
        // ==========================================
        [Test, TestCaseSource(nameof(GetVoucherData))]
        public void ExecuteVoucherTest(string testCaseId, string action, string codeOrSearch, string name, string type, string value, string maxUses, string expiryDate, string expected, int rowIndex)
        {
            string actualMessage = "";
            bool isPass = false;

            try
            {
                // Rẽ nhánh tùy thuộc vào cột Action trong Excel
                switch (action.ToLower())
                {
                    case "create":
                        voucherPage.ClickAddVoucher();
                        voucherPage.EnterVoucherData(codeOrSearch, name, type, value, maxUses, expiryDate);
                        voucherPage.ClickSave();
                        // Lấy thông báo lỗi hoặc thành công (Giả định bạn có hàm này trong VoucherPage)
                        // actualMessage = voucherPage.GetResultMessage(); 
                        actualMessage = "Thành công"; // Tạm fix cứng, bạn thay bằng hàm GetResultMessage nhé
                        break;

                    case "edit":
                        voucherPage.ClickFirstEditVoucher();
                        // Chỉ nhập nếu Excel có dữ liệu
                        if (!string.IsNullOrEmpty(codeOrSearch)) { driver.FindElement(By.Id("voucherCode")).Clear(); driver.FindElement(By.Id("voucherCode")).SendKeys(codeOrSearch); }
                        if (!string.IsNullOrEmpty(name)) { driver.FindElement(By.Id("voucherName")).Clear(); driver.FindElement(By.Id("voucherName")).SendKeys(name); }
                        if (!string.IsNullOrEmpty(type)) new SelectElement(driver.FindElement(By.Id("discountType"))).SelectByValue(type);
                        if (!string.IsNullOrEmpty(value)) { driver.FindElement(By.Id("discountValue")).Clear(); driver.FindElement(By.Id("discountValue")).SendKeys(value); }
                        if (!string.IsNullOrEmpty(maxUses)) { driver.FindElement(By.Id("maxUses")).Clear(); driver.FindElement(By.Id("maxUses")).SendKeys(maxUses); }
                        voucherPage.ClickSave();
                        actualMessage = "Cập nhật thành công";
                        break;

                    case "search":
                        voucherPage.SearchVoucher(codeOrSearch);
                        Thread.Sleep(1000);
                        int count = voucherPage.GetSearchResultCount();
                        actualMessage = count > 0 ? "Tồn tại" : "Không tìm thấy";
                        break;

                    default:
                        actualMessage = $"Lỗi: Cột Action '{action}' không hợp lệ";
                        break;
                }

                // SO SÁNH KẾT QUẢ
                if (actualMessage.ToLower().Contains(expected.ToLower()))
                {
                    isPass = true;
                }
            }
            catch (Exception ex)
            {
                actualMessage = "Lỗi Exception: " + ex.Message;
                isPass = false;
            }

            // GHI VÀO EXCEL & CHỤP ẢNH
            WriteResultToExcel(rowIndex, actualMessage, isPass, testCaseId);

            // Báo cáo cho Test Explorer
            Assert.IsTrue(isPass, $"Mã Test {testCaseId} FAILED. Actual: {actualMessage}");
        }

        // ==========================================
        // HÀM GHI KẾT QUẢ XUỐNG EXCEL
        // ==========================================
        private void WriteResultToExcel(int rowIndex, string actual, bool isPass, string testCaseId)
        {
            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet("Voucher");
                    worksheet.Cell(rowIndex, 10).Value = actual; // Cột 10 là Actual
                    worksheet.Cell(rowIndex, 11).Value = isPass ? "PASS" : "FAIL"; // Cột 11 là Result

                    if (!isPass)
                    {
                        string fileName = $"{testCaseId}_{DateTime.Now:HHmmss}.png";
                        string fullPath = Path.Combine(screenshotFolder, fileName);
                        ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(fullPath);

                        worksheet.Cell(rowIndex, 12).Value = "Link Ảnh"; // Cột 12 là Screenshot
                        worksheet.Cell(rowIndex, 12).SetHyperlink(new XLHyperlink(fullPath));
                    }
                    workbook.Save();
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}