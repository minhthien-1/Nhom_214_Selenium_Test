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
        private By txtSearch = By.Id("searchVoucher"); // LƯU Ý: Đổi ID này nếu web của bạn dùng ID khác cho ô tìm kiếm
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
            var editButtons = driver.FindElements(btnFirstEdit);
            if (editButtons.Count > 0) editButtons[0].Click();
        }

        public void EnterVoucherData(string code, string name, string type, string value, string maxUses, string expiryDate, string desc = "")
        {
            // Xóa dữ liệu cũ trước khi nhập (dùng cho cả Thêm và Sửa)
            driver.FindElement(txtVoucherCode).Clear();
            if (code != "") driver.FindElement(txtVoucherCode).SendKeys(code);

            driver.FindElement(txtVoucherName).Clear();
            if (name != "") driver.FindElement(txtVoucherName).SendKeys(name);

            // Chọn Loại giảm giá (fixed hoặc percentage)
            var typeSelect = new SelectElement(driver.FindElement(selectDiscountType));
            typeSelect.SelectByValue(type);

            driver.FindElement(txtDiscountValue).Clear();
            driver.FindElement(txtDiscountValue).SendKeys(value);

            driver.FindElement(txtMaxUses).Clear();
            driver.FindElement(txtMaxUses).SendKeys(maxUses);

            driver.FindElement(txtExpiryDate).Clear();
            driver.FindElement(txtExpiryDate).SendKeys(expiryDate);

            driver.FindElement(txtDescription).Clear();
            driver.FindElement(txtDescription).SendKeys(desc);
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
            searchInput.SendKeys(keyword);
        }

        public int GetSearchResultCount()
        {
            return driver.FindElements(voucherRows).Count;
        }
    }
}