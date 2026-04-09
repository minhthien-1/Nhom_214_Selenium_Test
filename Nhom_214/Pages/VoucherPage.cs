using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Nhom_214.Pages
{
    public class VoucherPage : BasePage
    {
        // 1. Khai báo Locators (Giống constructor trong TestCafe)
        private By btnAddVoucher = By.XPath("//button[contains(text(), 'Thêm Voucher')]");
        private By txtVoucherCode = By.Id("voucherCode");
        private By txtVoucherName = By.Id("voucherName");
        private By selectDiscountType = By.Id("discountType");
        private By txtDiscountValue = By.Id("discountValue");
        private By txtMaxUses = By.Id("maxUses");
        private By txtExpiryDate = By.Id("expiryDate");
        private By txtDescription = By.Id("description");
        private By selectStatus = By.Id("status");

        private By btnSave = By.CssSelector(".modal-footer button[type='submit']");
        private By btnCancel = By.CssSelector(".modal-footer .btn-secondary");
        private By btnFirstEdit = By.CssSelector(".btn-edit"); // Lấy nút sửa đầu tiên
        private By errorMessage = By.CssSelector(".error-message");

        // --- LOCATORS CHO PHẦN TÌM KIẾM ---
        private By txtSearch = By.Id("searchInput");
        private By voucherRows = By.CssSelector("table tbody tr");

        // Truyền driver từ BasePage
        private IWebDriver driver;

        public VoucherPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // 2. Các hàm tương tác (Methods)
        public void ClickAddVoucher()
        {
            driver.FindElement(btnAddVoucher).Click();
        }

        public void ClickFirstEditVoucher()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            // Bước 1: Đợi cho đến khi bảng dữ liệu (rows) xuất hiện
            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            // Bước 2: Tìm danh sách nút sửa
            var editButtons = driver.FindElements(btnFirstEdit);

            if (editButtons.Count > 0)
            {
                IWebElement firstBtn = editButtons[0];
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                // Cuộn tới nút
                js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", firstBtn);
                Thread.Sleep(500);

                try
                {
                    firstBtn.Click();
                }
                catch
                {
                    js.ExecuteScript("arguments[0].click();", firstBtn);
                }
            }
            else
            {
                // Nếu vẫn không thấy nút, có thể do trang trống. 
                // Hãy kiểm tra lại xem Admin của bạn đã tạo voucher nào chưa.
                throw new NoSuchElementException("Không tìm thấy nút chỉnh sửa nào có class .btn-edit. Hãy kiểm tra xem danh sách Voucher có dữ liệu chưa?");
            }
        }

        public void EnterVoucherData(string code, string name, string type, string value, string maxUses, string expiryDate, string desc = "")
        {
            // Cứ ô nào Excel KHÔNG TRỐNG thì mới nhập, còn trống thì bỏ qua để giữ nguyên dữ liệu cũ

            driver.FindElement(txtVoucherCode).Clear();
            if (!string.IsNullOrEmpty(code)) driver.FindElement(txtVoucherCode).SendKeys(code);

            driver.FindElement(txtVoucherName).Clear();
            if (!string.IsNullOrEmpty(name)) driver.FindElement(txtVoucherName).SendKeys(name);

            // CHỖ NÀY QUAN TRỌNG NHẤT: Chỉ chọn dropdown nếu có truyền type từ Excel
            if (!string.IsNullOrEmpty(type))
            {
                var typeSelect = new SelectElement(driver.FindElement(selectDiscountType));
                try
                {
                    typeSelect.SelectByValue(type.ToLower());
                }
                catch
                {
                    // Nếu không tìm thấy value (ví dụ fixed/percentage), thử chọn theo Text hiển thị
                    typeSelect.SelectByText(type);
                }
            }

            driver.FindElement(txtDiscountValue).Clear();
            if (!string.IsNullOrEmpty(value)) driver.FindElement(txtDiscountValue).SendKeys(value);

            driver.FindElement(txtMaxUses).Clear();
            if (!string.IsNullOrEmpty(maxUses)) driver.FindElement(txtMaxUses).SendKeys(maxUses);

            driver.FindElement(txtExpiryDate).Clear();
            if (!string.IsNullOrEmpty(expiryDate)) driver.FindElement(txtExpiryDate).SendKeys(expiryDate);

            driver.FindElement(txtDescription).Clear();
            if (!string.IsNullOrEmpty(desc)) driver.FindElement(txtDescription).SendKeys(desc);
        }

        public void SelectStatus(string statusValue)
        {
            var statusDropdown = new SelectElement(driver.FindElement(selectStatus));
            statusDropdown.SelectByValue(statusValue);
        }

        public void ClickSave()
        {
            driver.FindElement(btnSave).Click();
        }

        public void ClickCancel()
        {
            driver.FindElement(btnCancel).Click();
        }

        // --- METHODS CHO PHẦN TÌM KIẾM ---
        public void SearchVoucher(string keyword)
        {
            var searchInput = driver.FindElement(txtSearch);
            searchInput.Clear();

            // Gõ từ khóa vào
            searchInput.SendKeys(keyword);

            // Gửi thêm phím Tab để chắc chắn hệ thống nhận biết input đã thay đổi và thực hiện lọc
            searchInput.SendKeys(Keys.Tab);

            // Đợi một chút để danh sách kịp render lại (Thay vì dùng Thread.Sleep ở ngoài Test, ta xử lý ở đây cho sạch)
            System.Threading.Thread.Sleep(500);
        }

        public int GetSearchResultCount()
        {
            return driver.FindElements(voucherRows).Count;
        }
    }
}